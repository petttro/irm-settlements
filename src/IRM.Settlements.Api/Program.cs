using IRM.Settlements.Api.BackgroundServices;
using IRM.Settlements.Api.Endpoints;
using IRM.Settlements.Api.Extensions;
using IRM.Settlements.Api.Middlewares;
using IRM.Settlements.Api.ProblemDetails;
using IRM.Settlements.Application;
using IRM.Settlements.Infrastructure;
using JasperFx;
using Microsoft.FeatureManagement;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFeatureManagement();

const string defaultCorsPolicy = "DefaultCorsPolicy";

builder.Services
    .AddAuthenticationAndAuthorization(builder.Configuration)
    .AddSwagger(builder.Environment)
    .AddCors(builder.Configuration, defaultCorsPolicy)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddOpenTelemetryTracing(builder.Configuration)
    .AddCustomHealthChecks(builder.Environment)
    .AddProblemDetails(ProblemDetailsConfigurator.Configure)
    .AddHttpContextAccessor()
    .AddApiValidators();

builder.Services.AddHostedService<CheckPaymentStatusBackgroundService>();

// Без этого дефолтный провайдер ASP.NET пишет логи параллельно с Serilog.
builder.Logging.ClearProviders();
builder.WebHost.ConfigureSentry();

builder.Host
    .AddSerilog()
    .AddWolverine(builder.Configuration, builder.Environment);

var app = builder.Build();

if (!builder.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(swagger =>
    {
        swagger.ConfigObject = new ConfigObject { ShowCommonExtensions = true };
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(defaultCorsPolicy);
app.UseStatusCodePages();
app.UseExceptionHandler();
app.ConfigureExceptionsHandlers();
app.MapHealthChecks();
app.UseAuthentication();
app.UseMiddleware<UserLoggingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthorization();

app.MapDictionariesEndpoints();
app.MapReportsEndpoints();

// https://wolverinefx.net/guide/codegen.html
return await app.RunJasperFxCommands(args);
