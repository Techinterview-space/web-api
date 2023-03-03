using System;
using Domain.Exceptions;
using Domain.Validation;
using Domain.ValueObjects.Dates;
using Domain.ValueObjects.Dates.Interfaces;
using Xunit;

namespace DomainTests.Validation;

public class EntityValidatorExtensionsTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ThrowIfDateRangeIsNotValid_FromIsEarlierThanAllowedMin_Exception(bool isRequired)
    {
        var target = new AwesomeClass(
            TimeRange.Min.AddDays(-1),
            Date.Yesterday.EndOfTheDay());

        Assert.True(target.From.Earlier(TimeRange.Min));

        Assert.Throws<InvalidDateRangeException>(() => target.ThrowIfDateRangeIsNotValid(isRequired));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ThrowIfDateRangeIsNotValid_ToLaterThanAllowedMax_Exception(bool isRequired)
    {
        var target = new AwesomeClass(
            Date.Today.StartOfTheDay(),
            TimeRange.Max.AddDays(1));

        Assert.True(target.To.Later(TimeRange.Max));

        Assert.Throws<InvalidDateRangeException>(() => target.ThrowIfDateRangeIsNotValid(isRequired));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ThrowIfDateRangeIsNotValid_FromLaterThanTo_Exception(bool isRequired)
    {
        var target = new AwesomeClass(
            Date.Today.StartOfTheDay(),
            Date.Yesterday.EndOfTheDay());

        Assert.True(target.From.Later(target.ToOrFail()));

        Assert.Throws<InvalidDateRangeException>(() => target.ThrowIfDateRangeIsNotValid(isRequired));
    }

    [Fact]
    public void ThrowIfDateRangeIsNotValid_ToIsRequired_ToExists_NoException()
    {
        var target = new AwesomeClass();

        target.ThrowIfDateRangeIsNotValid(true);
    }

    [Fact]
    public void ThrowIfDateRangeIsNotValid_ToIsNotRequired_ToDoesNotExist_Ok()
    {
        var target = new AwesomeClass(Date.Yesterday.StartOfTheDay());

        target.ThrowIfDateRangeIsNotValid(false);
    }

    [Fact]
    public void ThrowIfDateRangeIsNotValid_ToIsRequired_ToDoesNotExist_Exception()
    {
        var target = new AwesomeClass(Date.Yesterday.StartOfTheDay());

        Assert.Throws<ArgumentNullException>(() => target.ThrowIfDateRangeIsNotValid(true));
    }

    private class AwesomeClass : IHasFromToDates
    {
        public AwesomeClass()
            : this(
                Date.Yesterday.StartOfTheDay(),
                Date.Today.EndOfTheDay())
        {
        }

        public AwesomeClass(DateTimeOffset from, DateTimeOffset? to = null)
        {
            From = from;
            To = to;
        }

        public DateTimeOffset From { get; set; }

        public DateTimeOffset? To { get; set; }
    }
}