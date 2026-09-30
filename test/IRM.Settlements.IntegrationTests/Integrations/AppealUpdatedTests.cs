using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Integrations.IRM;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Nuget.IRM.Kafka.Consumer.Consuming.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Integrations;

public class AppealUpdatedTests : BaseIntegrationTests
{
    public AppealUpdatedTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
        : base(factory, testOutput)
    {
    }

    [Fact]
    public async Task Should_Process_NewAppeal_Event()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        var json = await LoadFile("AppealUpdatedEvent.json");

        using var scope = WebApplicationFactory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IKafkaTopicMessageConsumer>() as FakeKafkaTopicMessageConsumer;

        // Act
        await consumer!.ProcessAsync(json);

        var appealId = 15727126;

        // Assert
        await Eventually(async () =>
        {
            var appeal = await GetAppealsFromDb(appealId);
            appeal.ShouldNotBeNull();

            appeal.Id.ShouldBe(appealId);
            appeal.BrandId.ShouldBe(2);
            appeal.CouponNumber.ShouldBe("410001692502");

            appeal.CityKisId.ShouldBe("0014");
            appeal.CityName.ShouldBe("Санкт-Петербург");

            appeal.Created.ShouldBe(DateTime.Parse("2026-04-20T15:33:51.803").ToUniversalTime());
            appeal.SaleDate.ShouldBe(DateTime.Parse("2026-04-20T18:29:39").ToUniversalTime());
            appeal.ServiceDate.ShouldBe(DateOnly.Parse("2026-04-22T00:00:00"));

            appeal.ServiceCompanyId.ShouldBe(18);
            appeal.ServiceCompany.SapId.ShouldBe("K000007490");
            appeal.ServiceCompany.Name.ShouldBe("ООО \"Градуат\"");

            appeal.ServiceCenterId.ShouldBe(1524);
            appeal.ServiceCenter.ExternalId.ShouldBe("31");
            appeal.ServiceCenter.Name.ShouldBe("\"СЦ Градуат 1\"");

            appeal.WareCode.ShouldBe("6010073");
            appeal.ServiceName.ShouldBe("Установка стиральной машины_6010082");

            appeal.ServicePrice.ShouldBe(1599);

            appeal.PaymentStatus.ShouldBe(PaymentStatus.Empty);
            appeal.BsiStatus.ShouldBe(BsiStatus.Complete);

            appeal.ProjectTypeId.ShouldBe(2);
            appeal.ShopName.ShouldBe("S105");

            appeal.AdditionalServices.Count.ShouldBe(2);
            appeal.AdditionalServices[0].Name.ShouldBe("Комф. Уст. Холод./Мор. с Перенав. дверей");
            appeal.AdditionalServices[0].WareCode.ShouldBe("6007247");
            appeal.AdditionalServices[1].Name.ShouldBe("Выезд мастера 1 зона");
            appeal.AdditionalServices[1].WareCode.ShouldBe("6010075");
        });
    }

    [Fact]
    public async Task Should_Process_ExistingAppeal_Event()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        var oldDate = DateTime.Parse("2026-04-20T15:33:51.803").ToUniversalTime();
        var oldEvent = CreateAppealUpdatedEvent(oldDate.AddDays(-10));
        await AddAppealToDb(oldEvent);

        var newEventJson = await LoadFile("AppealUpdatedEvent.json");
        using var scope = WebApplicationFactory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IKafkaTopicMessageConsumer>() as FakeKafkaTopicMessageConsumer;

        // Act
        await consumer!.ProcessAsync(newEventJson);

        // Assert
        await Eventually(async () =>
        {
            var appeal = await GetAppealsFromDb(oldEvent.AppealUpdatedMessage.Snapshot.Id);
            appeal.ShouldNotBeNull();

            appeal.Id.ShouldBe(15727126);
            appeal.BrandId.ShouldBe(2);
            appeal.CouponNumber.ShouldBe("410001692502");

            appeal.CityKisId.ShouldBe("0014");
            appeal.CityName.ShouldBe("Санкт-Петербург");

            appeal.Created.ShouldBe(DateTime.Parse("2026-04-20T15:33:51.803").ToUniversalTime());
            appeal.SaleDate.ShouldBe(DateTime.Parse("2026-04-20T18:29:39").ToUniversalTime());
            appeal.ServiceDate.ShouldBe(DateOnly.Parse("2026-04-22T00:00:00"));

            appeal.ServiceCompanyId.ShouldBe(18);
            appeal.ServiceCompany.SapId.ShouldBe("K000007490");
            appeal.ServiceCompany.Name.ShouldBe("ООО \"Градуат\"");

            appeal.ServiceCenterId.ShouldBe(1524);
            appeal.ServiceCenter.ExternalId.ShouldBe("31");
            appeal.ServiceCenter.Name.ShouldBe("\"СЦ Градуат 1\"");

            appeal.WareCode.ShouldBe("6010073");
            appeal.ServiceName.ShouldBe("Установка стиральной машины_6010082");

            appeal.ServicePrice.ShouldBe(1599);

            appeal.PaymentStatus.ShouldBe(PaymentStatus.Empty);
            appeal.BsiStatus.ShouldBe(BsiStatus.Complete);

            appeal.ProjectTypeId.ShouldBe(2);
            appeal.ShopName.ShouldBe("S105");

            appeal.AdditionalServices.Count.ShouldBe(2);
            appeal.AdditionalServices[0].Name.ShouldBe("Комф. Уст. Холод./Мор. с Перенав. дверей");
            appeal.AdditionalServices[0].WareCode.ShouldBe("6007247");
            appeal.AdditionalServices[1].Name.ShouldBe("Выезд мастера 1 зона");
            appeal.AdditionalServices[1].WareCode.ShouldBe("6010075");
        });
    }

    [Fact]
    public async Task Should_Not_Process_ExistingAppeal_OlderEvent()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        var oldDate = DateTime.Parse("2026-04-20T15:33:51.803").ToUniversalTime();
        var oldEvent = CreateAppealUpdatedEvent(oldDate.AddDays(10));
        await AddAppealToDb(oldEvent);

        var newEventJson = await LoadFile("AppealUpdatedEvent.json");
        using var scope = WebApplicationFactory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IKafkaTopicMessageConsumer>() as FakeKafkaTopicMessageConsumer;

        // Act
        await consumer!.ProcessAsync(newEventJson);

        // Assert
        await Eventually(async () =>
        {
            var expectedAppeal = oldEvent.AppealUpdatedMessage.Snapshot;
            var appeal = await GetAppealsFromDb(oldEvent.AppealUpdatedMessage.Snapshot.Id);
            appeal.ShouldNotBeNull();

            appeal.BrandId.ShouldBe(expectedAppeal.BrandId);
            appeal.CouponNumber.ShouldBe(expectedAppeal.CouponNumber);

            appeal.CityKisId.ShouldBe(expectedAppeal.CityKisId);
            appeal.CityName.ShouldBe(expectedAppeal.CityName);

            appeal.Created.ShouldBe(expectedAppeal.Created);
            appeal.SaleDate.ShouldBe(expectedAppeal.SaleDate);
            appeal.ServiceDate.ShouldBe(DateOnly.FromDateTime(expectedAppeal.ServiceDate!.Value));

            appeal.ServiceCompanyId.ShouldBe(expectedAppeal.ServiceCompanyId);
            appeal.ServiceCompany.SapId.ShouldBe(expectedAppeal.EnterpriseId);
            appeal.ServiceCompany.Name.ShouldBe(expectedAppeal.ServiceCompanyName);

            appeal.ServiceCenterId.ShouldBe(expectedAppeal.ResolverId);
            appeal.ServiceCenter.ExternalId.ShouldBe(expectedAppeal.ResolverExternalId);
            appeal.ServiceCenter.Name.ShouldBe(expectedAppeal.ResolverNameResolver);

            appeal.WareCode.ShouldBe(expectedAppeal.WareCode);
            appeal.ServiceName.ShouldBe(expectedAppeal.ServiceName);

            appeal.ServicePrice.ShouldBe(expectedAppeal.ServicePrice);

            appeal.PaymentStatus.ShouldBe((PaymentStatus)expectedAppeal.PaymentStatus);
            appeal.BsiStatus.ShouldBe((BsiStatus)expectedAppeal.BsiStatus);

            appeal.ProjectTypeId.ShouldBe(expectedAppeal.ProjectTypeId);
            appeal.ShopName.ShouldBe(expectedAppeal.ShopName);

            appeal.AdditionalServices.Count.ShouldBe(1);
            appeal.AdditionalServices[0].Name.ShouldBe("New Additional Service");
            appeal.AdditionalServices[0].WareCode.ShouldBe("001122");
        });
    }

    [Fact]
    public async Task Should_Process_Multiple_Events()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        var json = await LoadFile("KafkaDevMessages.json");
        var arr = JArray.Parse(json);

        var messages
            = arr
                .Select(x => x.ToString(Formatting.Indented))
                .ToArray();

        using var scope = WebApplicationFactory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IKafkaTopicMessageConsumer>() as FakeKafkaTopicMessageConsumer;

        // Act
        foreach (var message in messages)
        {
            await consumer!.ProcessAsync(message);
        }

        var appealsRepository = scope.ServiceProvider.GetRequiredService<IAppealRepository>();

        await Eventually(async () =>
        {
            var appeals = await appealsRepository.GetAllAsync();
            appeals.Count().ShouldBe(3);
        });
    }

    private AppealUpdatedEvent CreateAppealUpdatedEvent(DateTime eventCreatedAt)
    {
        var eventModel = new AppealUpdatedMessage
        {
            CreatedAt = eventCreatedAt,
            Snapshot = new AppealUpdatedSnapshot
            {
                Id = 15727126,
                BrandId = 2,
                CouponNumber = "410001692502",
                SaleOrderNumber = "234234324234",
                CityKisId = "0033",
                CityName = "Москва",

                Created = DateTime.Parse("2026-01-20T15:33:51.803").ToUniversalTime(),
                SaleDate = DateTime.Parse("2026-02-20T18:29:39").ToUniversalTime(),
                ServiceDate = DateTime.Parse("2026-03-22T00:00:00"),
                AppealDate = DateTime.Parse("2026-05-20T15:29:39").ToUniversalTime(),
                ServiceCompanyId = 18,
                EnterpriseId = "410001692502",
                ServiceCompanyName = "ООО \"Рога и копыта\"",

                ResolverId = 1524,
                ResolverExternalId = "67",
                ResolverNameResolver = "Сервис центр какой-то",

                WareCode = "3310073",
                ServiceName = "Установка посудомоечной машины_6010082",
                ServicePrice = 2399,
                PaymentStatus = (int)PaymentStatus.AddedToTheReport,
                BsiStatus = (int)BsiStatus.Cancel,
                ProjectTypeId = 2,
                ShopId = 300,
                ShopName = "S405",
                AdditionalServices = [
                    new AdditionalServiceMessage
                    {
                        Name = "New Additional Service",
                        SapId = "001122"
                    }
                ]
            }
        };

        var updatedEvent = new AppealUpdatedEvent(eventModel);

        return updatedEvent;
    }
}
