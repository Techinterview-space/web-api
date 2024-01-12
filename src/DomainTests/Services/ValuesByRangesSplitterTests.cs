using Domain.Services;
using Xunit;

namespace DomainTests.Services;

public class ValuesByRangesSplitterTests
{
    [Theory]
    [InlineData(0, 100, 10, 10)]
    [InlineData(450, 12300, 500, 24)]
    public void ToList_Cases_Match(
        double min,
        double max,
        int step,
        int expectedCount)
    {
        var target = new ValuesByRangesSplitter(min, max, step);
        var actual = target.ToList();
        Assert.Equal(expectedCount, actual.Count);
        Assert.Equal(min, actual[0].Start);
        Assert.Equal(min + step, actual[0].End);
        Assert.Equal(max, actual[^1].End);
    }
}