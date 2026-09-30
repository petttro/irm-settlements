using ClosedXML.Excel;
using IRM.Settlements.IntegrationTests.Fixtures;
using Nuget.IRM.Tools.Enum.Utils.AttributesProcessing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class ExportReportsTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task ExportReports_Success()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        var reports = await CreateDefaultReports();

        // Act
        var response = await HttpClient.GetAsync($"reports/export/?serviceCompanyId={ServiceCompanyId}" );
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var bytes = await response.Content.ReadAsByteArrayAsync();

        bytes.Length.ShouldBeGreaterThan(0);

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);

        var sheet = workbook.Worksheets.First();

        var headers = new[]
        {
            "Название",
            "Контрагент",
            "Дата создания отчета",
            "Дата отправки на оплату",
            "Дата оплаты",
            "Статус отчета",
            "Количество позиций",
            "Сумма"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(3, i + 1).GetString().ShouldBe(headers[i]);
        }

        var orderedReports = reports.OrderByDescending(r => r.CreatedAt).ToList();

        for (var i = 0; i < 3; i++)
        {
            var rowIndex = 4 + i;
            sheet.Cell(rowIndex, 1).GetString().ShouldBe(orderedReports[i].Name);
            sheet.Cell(rowIndex, 2).GetString().ShouldBe(orderedReports[i].ServiceCompanyName);
            sheet.Cell(rowIndex, 3).GetDateTime().ShouldBe(orderedReports[i].CreatedAt, TimeSpan.FromMilliseconds(1));
            sheet.Cell(rowIndex, 4).GetString().ShouldBe(string.Empty);
            sheet.Cell(rowIndex, 5).GetString().ShouldBe(string.Empty);
            sheet.Cell(rowIndex, 6).GetString().ShouldBe(orderedReports[i].Status.GetDescription());
            sheet.Cell(rowIndex, 7).GetString().ShouldBe(orderedReports[i].ItemsCount.ToString());
            sheet.Cell(rowIndex, 8).GetDouble().ShouldBe((double)orderedReports[i].TotalCost);
        }

        var filePath = Path.Combine(Path.GetTempPath(), "reports.xlsx");
        await File.WriteAllBytesAsync(filePath, bytes);

        TestOutput.WriteLine($"File saved to: {filePath}");
    }
}
