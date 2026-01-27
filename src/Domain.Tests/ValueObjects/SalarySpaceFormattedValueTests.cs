using Domain.ValueObjects;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class SalarySpaceFormattedValueTests
{
    [Theory]
    [InlineData(999, "999")]
    [InlineData(735_000, "735 000")]
    [InlineData(1_735_000, "1 735 000")]
    [InlineData(2_000_000, "2 000 000")]
    public void ToString_Cases_Match(
        double source,
        string expected)
    {
        Assert.Equal(expected, new SalarySpaceFormattedValue(source).ToString());
    }
}