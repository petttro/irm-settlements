using System.Net;
using System.Net.Http.Json;
using IRM.Settlements.Api.Auth.Handlers;
using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.IrmCatalog;
using IRM.Settlements.Infrastructure.IrmCatalog.HttpClients;
using IRM.Settlements.Infrastructure.PaymentGateway.Contracts;
using IRM.Settlements.Infrastructure.PaymentGateway.HttpClients;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using IRM.Settlements.Infrastructure.SAP.Clients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nuget.IRM.Kafka.Consumer.Consuming.Interfaces;
using Npgsql;
using Xunit;

namespace IRM.Settlements.IntegrationTests.Fixtures;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public readonly PostgresFixture PostgresFixture = new();

    public HttpResponseMessage CurrentHttpResponse { get; set; } = new HttpResponseMessage(HttpStatusCode.OK);


    public async Task InitializeAsync()
    {
        await PostgresFixture.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await PostgresFixture.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTests");
        Environment.SetEnvironmentVariable("KafkaConsumerSettings__0__ClusterSettings__BootstrapServers", "localhost:9092");
        Environment.SetEnvironmentVariable("KafkaConsumerSettings__0__Topics__0__TopicName", "test-topic");
        Environment.SetEnvironmentVariable("KafkaConsumerSettings__0__Topics__0__ConsumerGroupId", "test-group");
        Environment.SetEnvironmentVariable("KafkaProducerSettings__0__ClusterSettings__BootstrapServers", "localhost:9092");
        Environment.SetEnvironmentVariable("KafkaProducerSettings__0__Topics__0__TopicName", "test-topic");
        Environment.SetEnvironmentVariable("ConnectionStrings__SettlementsConnectionString", PostgresFixture.ConnectionString);
        Environment.SetEnvironmentVariable("ApiKeySettings__ApiKey", "test-api-key");

        builder.UseEnvironment("IntegrationTests");

        builder.ConfigureServices(services =>
        {
            // Переопределяем аутентификацию
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { })
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                    "TestApiKey", options => { options.ApiKey = "test-api-key"; });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("JwtOrApiKey", policy =>
                {
                    policy
                        .AddAuthenticationSchemes("Test", "TestApiKey")
                        .RequireAuthenticatedUser();
                });
            });

            ReplaceDescriptor<IMessagePublisher, FakeMessagePublisher>(services);
            ReplaceDescriptor<IKafkaTopicMessageConsumer, FakeKafkaTopicMessageConsumer>(services);
            ReplaceDescriptor<ISapClient, FakeSapClient>(services);
            ReplaceDescriptor<IIrmCatalogService, FakeIrmCatalogService>(services);

            RemoveDbContexts(services);

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(PostgresFixture.ConnectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            services.AddDbContext<SettlementsDbContext>((_, options) =>
            {
                options.EnableSensitiveDataLogging();
                options.UseNpgsql(dataSource);
            });


            services.RemoveAll<PaymentOrderClient>();
            services
                .AddHttpClient<PaymentOrderClient>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new FakeHttpMessageHandler(_ =>
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent("{\"result\":\"Все хорошо!\"}")
                        };
                    }));

            services.RemoveAll<PaymentStatusClient>();
            services
                .AddHttpClient<PaymentStatusClient>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new FakeHttpMessageHandler(req =>
                    {
                        switch (req.RequestUri!.AbsolutePath)
                        {
                            case "/api/v1/dictionary":
                                return new HttpResponseMessage(HttpStatusCode.OK)
                                {
                                    Content = JsonContent.Create(new List<PaymentStatusResponse>
                                    {
                                        new()
                                        {
                                            Name = "Какой-то статус, главное финальный и успешный",
                                            Id = 1,
                                            IsFinal = true,
                                            IsPositive = true
                                        }
                                    })
                                };
                            case "/api/v1/payment/array":
                                return CurrentHttpResponse;
                            default:
                                return new HttpResponseMessage(HttpStatusCode.NotFound);
                        }
                    }));

            services.RemoveAll<IrmCatalogHttpClient>();
            services
                .AddHttpClient<IrmCatalogHttpClient>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new FakeHttpMessageHandler(_ =>
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = JsonContent.Create(new ServiceCompanyCatalogResponse
                            {
                                SapId = "sap-id",
                                Name = "name",

                                SapContractNumber = "sap-contract-number",
                                ExternalContractNumber = "contract-number",
                                ContractDate = DateOnly.FromDateTime(DateTime.UtcNow),

                                BankAccount = "bankaccount",
                                BankBic = "bic",
                                Inn = "inn",
                                Kpp = "kpp",
                                BankName = "bankname",
                                BankSwiftCode = "swiftcode",
                                CorrespondentAccount = "correspondentaccount",
                            })
                        };
                    }));
        });
    }

    private static void ReplaceDescriptor<TI, T>(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(TI));

        if (descriptor != null)
            services.Remove(descriptor);

        services.AddSingleton(typeof(TI), typeof(T));
    }

    private static void RemoveDbContexts(IServiceCollection services)
    {
        var descriptors = services
            .Where(s =>
                s.ServiceType == typeof(DbContextOptions<SettlementsDbContext>) ||
                s.ServiceType == typeof(SettlementsDbContext))
            .ToList();

        foreach (var d in descriptors)
        {
            services.Remove(d);
        }
    }
}
