using Domain.Services;
using Xunit;

namespace InfrastructureTests.Services;

public class RoundedValuesByRangesSplitterTests
{
    [Theory]
    [InlineData(0, 100, 10, 10)]
    [InlineData(450, 12300, 500, 25)]
    public void ToList_Cases_Match(
        double min,
        double max,
        int step,
        int expectedCount)
    {
        var target = new RoundedValuesByRangesSplitter(min, max, step);
        var actual = target.ToList();
        Assert.Equal(expectedCount, actual.Count);
    }
}