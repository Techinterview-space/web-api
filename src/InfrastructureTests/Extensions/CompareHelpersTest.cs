using System;
using Domain.Extensions;
using Domain.ValueObjects.Dates;
using Xunit;

namespace InfrastructureTests.Extensions;

public class CompareHelpersTest
{
    [Fact]
    public void SameMonth_SameMonths_True()
    {
        var firstOfTheMay = new DateTimeOffset(new DateTime(2020, 5, 1));
        var fifthOfTheMay = new DateTimeOffset(new DateTime(2020, 5, 5));

        Assert.True(firstOfTheMay.SameMonth(fifthOfTheMay));
    }

    [Fact]
    public void SameMonth_DifferentMonths_False()
    {
        var firstOfTheMay = new DateTimeOffset(new DateTime(2020, 4, 1));
        var fifthOfTheMay = new DateTimeOffset(new DateTime(2020, 5, 5));

        Assert.False(firstOfTheMay.SameMonth(fifthOfTheMay));
    }

    [Fact]
    public void LaterOrEqual_DifferentCases_Ok()
    {
        var firstOfTheMay = new DateTimeOffset(new DateTime(2020, 5, 1));
        var fifthOfTheMay = new DateTimeOffset(new DateTime(2020, 5, 5));

        Assert.True(fifthOfTheMay.LaterOrEqual(fifthOfTheMay));
        Assert.True(fifthOfTheMay.LaterOrEqual(firstOfTheMay));
        Assert.False(firstOfTheMay.LaterOrEqual(fifthOfTheMay));
    }

    [Fact]
    public void EarlierOrEqual_DifferentCases_Ok()
    {
        var firstOfTheMay = new DateTimeOffset(new DateTime(2020, 5, 1));
        var fifthOfTheMay = new DateTimeOffset(new DateTime(2020, 5, 5));

        Assert.True(fifthOfTheMay.EarlierOrEqual(fifthOfTheMay));
        Assert.False(fifthOfTheMay.EarlierOrEqual(firstOfTheMay));
        Assert.True(firstOfTheMay.EarlierOrEqual(fifthOfTheMay));
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData(null, 1d, false)]
    [InlineData(1d, null, false)]
    [InlineData(1d, 1d, true)]
    public void EqualTo_DoubleNullable_Ok(double? first, double? second, bool expected)
    {
        Assert.Equal(expected, first.EqualTo(second));
    }

    [Fact]
    public void IsToday_DifferentCases_Ok()
    {
        Assert.False(Date.Yesterday.EndOfTheDay().IsToday());

        Assert.False(Date.Yesterday.StartOfTheDay().IsToday());

        Assert.False(Date.Tomorrow.EndOfTheDay().IsToday());

        Assert.False(Date.Tomorrow.StartOfTheDay().IsToday());

        Assert.True(Date.Today.EndOfTheDay().IsToday());

        Assert.True(Date.Today.StartOfTheDay().IsToday());

        Assert.True(DateTimeOffset.Now.IsToday());
    }
}