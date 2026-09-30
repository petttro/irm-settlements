using IRM.Settlements.Infrastructure.Common.Extensions;

namespace IRM.Settlements.UnitTests.Infrastucture.Extensions;

public class DateTimeExtensionsTests
{
    [Theory]
    [InlineData("2026-04-20T15:33:51.803")]
    [InlineData("2026-04-20T18:29:39")]
    [InlineData("2026-04-22T00:00:00")]
    public void ParseDateTime_ToUtc(string dateString)
    {
        // Arrange
        var local = DateTime.SpecifyKind(DateTime.Parse(dateString), DateTimeKind.Unspecified);

        // Act
        var actual = local.MskToUtcMaybe();

        // Expected (ручной расчёт)
        var expected = local.AddHours(-3);

        // Assert
        Assert.Equal(expected, actual);
    }
}
