using IRM.Settlements.Application.UseCases.Reports.Commands;
using IRM.Settlements.Infrastructure.PaymentGateway.Settings;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Wolverine;

namespace IRM.Settlements.Api.BackgroundServices;

public sealed class CheckPaymentStatusBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CheckPaymentStatusBackgroundService> _logger;
    private readonly IFeatureManager _featureManager;

    private const string PaymentGatewayFeatureFlag = "UsePaymentGateway";

    private readonly TimeSpan _interval;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public CheckPaymentStatusBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<Finance3PSettings> finance3PSettings,
        ILogger<CheckPaymentStatusBackgroundService> logger,
        IFeatureManager featureManager)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _featureManager = featureManager;
        _interval = TimeSpan.FromSeconds(finance3PSettings.Value.CheckPaymentStatusIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await _featureManager.IsEnabledAsync(PaymentGatewayFeatureFlag))
            return;

        _logger.LogInformation($"{nameof(CheckPaymentStatusBackgroundService)} started");

        using var timer = new PeriodicTimer(_interval);

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (!await _lock.WaitAsync(0, stoppingToken))
            {
                _logger.LogWarning("Previous execution still running, skipping tick");
                continue;
            }

            try
            {
                SentrySdk.AddBreadcrumb(
                    message: "Starting payment status polling",
                    category: "background.job");

                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing payment polling");

                SentrySdk.CaptureException(ex, scope =>
                {
                    scope.SetTag("job", "check-payment-status");
                    scope.SetTag("component", "background-service");
                    scope.SetExtra("interval_seconds", _interval.TotalSeconds);
                });
            }
            finally
            {
                _lock.Release();
            }
        }

        _logger.LogInformation("PaymentPollingBackgroundService stopped");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // чтобы ошибки успели уйти в Sentry
        await SentrySdk.FlushAsync(TimeSpan.FromSeconds(2));
        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        _logger.LogInformation("Polling payment gateway...");

        using var scope = _scopeFactory.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        await bus.InvokeAsync(new CheckPaymentsCommand(), ct);
    }
}
