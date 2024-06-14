using System;
using Domain.ValueObjects;
using Xunit;

namespace InfrastructureTests.Services;

public class DateTimeRangeSplitterTests
{
    [Theory]
    [InlineData(15, 96)]
    [InlineData(30, 48)]
    [InlineData(60, 24)]
    [InlineData(120, 12)]
    [InlineData(180, 8)]
    [InlineData(240, 6)]
    [InlineData(300, 5)]
    [InlineData(360, 4)]
    [InlineData(480, 3)]
    [InlineData(720, 2)]
    [InlineData(1140, 2)]
    [InlineData(1440, 1)]
    public void Ctor_Cases_Match(
        int intervalInMinutes,
        int expectedCount)
    {
        var start = new DateTime(2024, 1, 12, 0, 0, 0);
        var target = new DateTimeRangeSplitter(
                start,
                start.AddHours(23).AddMinutes(59),
                TimeSpan.FromMinutes(intervalInMinutes))
            .ToList();

        Assert.Equal(expectedCount, target.Count);
    }
}