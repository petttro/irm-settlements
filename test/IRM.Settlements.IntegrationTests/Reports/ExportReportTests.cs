using IRM.Settlements.IntegrationTests.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class ExportReportTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task ExportReport_Success()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        var report= await CreateDefaultReport();

        // Act
        var response = await HttpClient.GetAsync($"reports/{report.Id}/export" );
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var bytes = await response.Content.ReadAsByteArrayAsync();

        bytes.Length.ShouldBeGreaterThan(0);

        var filePath = Path.Combine(Path.GetTempPath(), "report.xlsx");
        await File.WriteAllBytesAsync(filePath, bytes);

        TestOutput.WriteLine($"File saved to: {filePath}");
    }
}

