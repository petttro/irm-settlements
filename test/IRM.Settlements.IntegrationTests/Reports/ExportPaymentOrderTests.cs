using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using ClosedXML.Excel;
using IRM.Settlements.Api.Contracts.Reports;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class ReportsTestData
{
    public List<Report> Reports { get; } = [];
}

public class ExportPaymentOrderTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task ExportPaymentOrder_Success()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);
        await InitializeTestData(testData!);

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new PrivateSetterContractResolver()
        };

        var reportsJson = await LoadFile("PaymentOrderReportsData.json");
        var testReportData = JsonConvert.DeserializeObject<ReportsTestData>(reportsJson, settings);
        testReportData.ShouldNotBeNull();

        foreach (var report in testReportData.Reports)
        {
            report.MarkAsSendToPayment(Guid.NewGuid());
        }

        await SaveReportsToDb(testReportData.Reports);

        HttpClient
            .WithUser("Admin")
            .WithRoles(IrmRoles.Administrator);

        var request = new ExportPaymentOrderRequest
        {
            ReportIds = testReportData.Reports.Select(r => r.Id).ToList()
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("payment-orders/export", request);
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var bytes = await response.Content.ReadAsByteArrayAsync();

        bytes.Length.ShouldBeGreaterThan(0);

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);

        var sheet = workbook.Worksheets.First();

        sheet.Cell(2, 1).GetString().ShouldBe("SC001");
        sheet.Cell(2, 2).GetString().ShouldBe("S001");
        sheet.Cell(2, 3).GetString().ShouldBe("К");
        sheet.Cell(2, 4).GetString().ShouldBe("990000732");
        sheet.Cell(2, 5).GetString().ShouldBe("1");
        sheet.Cell(2, 6).GetString().ShouldBe("ЕР");
        sheet.Cell(2, 7).GetDouble().ShouldBe(420);
        sheet.Cell(2, 9).GetString().ShouldBe("MVZ-S001");
        sheet.Cell(2, 10).GetString().ShouldBe("sap-contract-number-SC001");
        sheet.Cell(2, 20).GetString().ShouldBe("IRM00001/2026");


        sheet.Cell(3, 1).GetString().ShouldBe("SC001");
        sheet.Cell(3, 2).GetString().ShouldBe("S002");
        sheet.Cell(3, 3).GetString().ShouldBe("К");
        sheet.Cell(3, 4).GetString().ShouldBe("990000732");
        sheet.Cell(3, 5).GetString().ShouldBe("1");
        sheet.Cell(3, 6).GetString().ShouldBe("ЕР");
        sheet.Cell(3, 7).GetDouble().ShouldBe(150);
        sheet.Cell(3, 9).GetString().ShouldBe("MVZ-S002");
        sheet.Cell(3, 10).GetString().ShouldBe("sap-contract-number-SC001");
        sheet.Cell(3, 20).GetString().ShouldBe("IRM00001/2026");

        sheet.Cell(4, 1).GetString().ShouldBe("SC001");
        sheet.Cell(4, 2).GetString().ShouldBe("S003");
        sheet.Cell(4, 3).GetString().ShouldBe("К");
        sheet.Cell(4, 4).GetString().ShouldBe("990000732");
        sheet.Cell(4, 5).GetString().ShouldBe("1");
        sheet.Cell(4, 6).GetString().ShouldBe("ЕР");
        sheet.Cell(4, 7).GetDouble().ShouldBe(130);
        sheet.Cell(4, 9).GetString().ShouldBe("");
        sheet.Cell(4, 10).GetString().ShouldBe("sap-contract-number-SC001");
        sheet.Cell(4, 20).GetString().ShouldBe("IRM00001/2026");

        sheet.Cell(5, 1).GetString().ShouldBe("SC002");
        sheet.Cell(5, 2).GetString().ShouldBe("S001");
        sheet.Cell(5, 3).GetString().ShouldBe("К");
        sheet.Cell(5, 4).GetString().ShouldBe("990001077");
        sheet.Cell(5, 5).GetString().ShouldBe("1");
        sheet.Cell(5, 6).GetString().ShouldBe("ЕР");
        sheet.Cell(5, 7).GetDouble().ShouldBe(300);
        sheet.Cell(5, 9).GetString().ShouldBe("MVZ-S001");
        sheet.Cell(5, 10).GetString().ShouldBe("sap-contract-number-SC002");
        sheet.Cell(5, 20).GetString().ShouldBe("IRM00002/2026");

        sheet.Cell(6, 1).GetString().ShouldBe("SC002");
        sheet.Cell(6, 2).GetString().ShouldBe("S002");
        sheet.Cell(6, 3).GetString().ShouldBe("К");
        sheet.Cell(6, 4).GetString().ShouldBe("990001077");
        sheet.Cell(6, 5).GetString().ShouldBe("1");
        sheet.Cell(6, 6).GetString().ShouldBe("ЕР");
        sheet.Cell(6, 7).GetDouble().ShouldBe(700);
        sheet.Cell(6, 9).GetString().ShouldBe("MVZ-S002");
        sheet.Cell(6, 10).GetString().ShouldBe("sap-contract-number-SC002");
        sheet.Cell(6, 20).GetString().ShouldBe("IRM00002/2026");

        var filePath = Path.Combine(Path.GetTempPath(), "payment-order.xlsx");
        await File.WriteAllBytesAsync(filePath, bytes);

        TestOutput.WriteLine($"File saved to: {filePath}");
    }

    [Fact]
    public async Task ExportPaymentOrder_ReportIdsNotSet_ReturnsBadRequest()
    {
        // Arrange
        HttpClient
            .WithUser("Admin")
            .WithRoles(IrmRoles.Administrator);

        var request = new ExportPaymentOrderRequest
        {
            ReportIds = []
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("payment-orders/export", request);
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("ReportIds");
    }
}

public sealed class PrivateSetterContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.Writable)
            return property;

        if (member is PropertyInfo propertyInfo &&
            propertyInfo.SetMethod is not null)
        {
            property.Writable = true;
        }

        return property;
    }
}
