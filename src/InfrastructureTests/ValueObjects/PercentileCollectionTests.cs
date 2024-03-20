using System.Collections.Generic;
using Domain.ValueObjects;
using Xunit;

namespace InfrastructureTests.ValueObjects;

public class PercentileCollectionTests
{
    [Fact]
    public void ToList_ReturnsCorrectPercentile()
    {
        var source = new List<int> { 1, 1, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 4, 5, 5, 6, 6, 6, 6, 7, 8, 8, 9, 10 };
        var percentileCollection = new PercentileCollection<int>(10, 90, source);
        var result = percentileCollection.ToList();

        Assert.Equal(19, result.Count);
        Assert.Equal(new List<int> { 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 4, 5, 5, 6, 6, 6, 6, 7, 8, }, result);
    }
}