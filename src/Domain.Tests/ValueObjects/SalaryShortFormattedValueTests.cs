using Domain.ValueObjects;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class SalaryShortFormattedValueTests
{
    [Theory]
    [InlineData(999, "999")]
    [InlineData(735_000, "735к")]
    [InlineData(1_735_000, "1.7млн")]
    [InlineData(2_000_000, "2млн")]
    public void ToString_Cases_Match(
        double source,
        string expected)
    {
        Assert.Equal(expected, new SalaryShortFormattedValue(source).ToString());
    }
}