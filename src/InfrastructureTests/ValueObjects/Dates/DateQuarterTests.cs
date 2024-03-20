using System;
using Domain.ValueObjects.Dates;
using Xunit;

namespace InfrastructureTests.ValueObjects.Dates;

public class DateQuarterTests
{
    [Theory]
    [InlineData(2024, 2024, 1, 1)]
    [InlineData(2023, 2023, 3, 1)]
    [InlineData(2024, 2024, 5, 2)]
    [InlineData(2024, 2024, 7, 3)]
    [InlineData(2024, 2024, 11, 4)]
    public void Ctor_Cases_Match(
        int year,
        int yearExpected,
        int month,
        int quarterExpected)
    {
        var date = new DateTimeOffset(
            year,
            month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);

        var target = new DateQuarter(date);
        Assert.Equal(yearExpected, target.Year);
        Assert.Equal(quarterExpected, target.Quarter);
    }
}