using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ValueObjects.Dates;
using Xunit;
using MonthRange = Domain.ValueObjects.Dates.MonthRange;

namespace InfrastructureTests.ValueObjects.Dates
{
    public class MonthRangeTest
    {
        [Theory]
        [InlineData(1, 23)]
        [InlineData(2, 20)]
        [InlineData(8, 21)]
        public void WorkDaysCount_DifferentValues_Ok(int month, int daysExpected)
        {
            Assert.Equal(daysExpected, new MonthRange(2020, month).WorkDaysCount());
        }

        [Theory]
        [InlineData(1, 30, 30)]
        [InlineData(5, 20, 16)]
        [InlineData(1, 1, 1)]
        public void Ctor_SameMonths_Ok(int start, int end, int daysCountExpected)
        {
            var target = new MonthRange(
                new Date(2020, 6, start),
                new Date(2020, 6, end));

            Assert.Equal(2020, target.Year);
            Assert.Equal(6, target.Month);
            Assert.Equal(daysCountExpected, target.DaysCount);
        }

        [Fact]
        public void Ctor_DifferentMonths_Exception()
        {
            Assert.Throws<ArgumentException>(() => new MonthRange(
                new Date(2020, 6, 1),
                new Date(2020, 7, 5)));
        }

        [Fact]
        public void Ctor_SameMonthsDifferentYears_Exception()
        {
            Assert.Throws<ArgumentException>(() => new MonthRange(
                new Date(2020, 6, 1),
                new Date(2021, 6, 30)));
        }

        [Fact]
        public void RemoveRanges_NotAllRangesIncludedIntoMainOne_Case1_Ok()
        {
            var target = new MonthRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            var rangesToRemove = new List<TimeRange>
            {
                new TimeRange(new Date(2020, 6, 15), new Date(2020, 7, 20)),
            };

            var ranges = target.RemoveRanges(rangesToRemove);

            Assert.Single(ranges);

            var newTimeRange = new TimeRange(new Date(2020, 6, 1), new Date(2020, 6, 14));
            Assert.Equal(newTimeRange.From, ranges.First().From);
            Assert.Equal(newTimeRange.To, ranges.First().To);
        }

        [Fact]
        public void RemoveRanges_NotAllRangesIncludedIntoMainOne_Case2_Ok()
        {
            var target = new MonthRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            var rangesToRemove = new List<TimeRange>
            {
                new TimeRange(new Date(2020, 5, 15), new Date(2020, 6, 14)),
            };

            var ranges = target.RemoveRanges(rangesToRemove);

            Assert.Single(ranges);

            var newTimeRange = new TimeRange(new Date(2020, 6, 15), new Date(2020, 6, 30));
            Assert.Equal(newTimeRange.From, ranges.First().From);
            Assert.Equal(newTimeRange.To, ranges.First().To);
        }

        [Fact]
        public void RemoveRanges_RangeNot_IntersectedWithMain_Exception()
        {
            var target = new MonthRange(new Date(2020, 12, 1), new Date(2020, 12, 15));
            var rangesToRemove = new List<TimeRange>
            {
                new TimeRange(new Date(2020, 11, 16), new Date(2020, 11, 28))
            };

            Assert.Throws<InvalidOperationException>(() => target.RemoveRanges(rangesToRemove));
        }

        [Fact]
        public void RemoveRanges_RangeInMainRange_Ok()
        {
            var target = new MonthRange(new Date(2020, 11, 5), new Date(2020, 11, 30));
            var vacationTimeRange = new TimeRange(new Date(2020, 11, 16), new Date(2020, 11, 28));

            var rangesToRemove = new List<TimeRange>
            {
                vacationTimeRange
            };

            var ranges = target.RemoveRanges(rangesToRemove);
            Assert.Equal(2, ranges.Count());
            var firstRange = ranges.First();
            Assert.Equal(target.From, firstRange.From);
            Assert.Equal(new Date(vacationTimeRange.From.AddDays(-1)).EndOfTheDay(), firstRange.To);

            var secondRange = ranges.Last();
            Assert.Equal(new Date(vacationTimeRange.To.AddDays(1)).StartOfTheDay(), secondRange.From);
            Assert.Equal(new Date(target.To).EndOfTheDay(), secondRange.To);
        }

        [Fact]
        public void RemoveRanges_RangeIntersect_WithMainRange_Ok()
        {
            var target = new MonthRange(new Date(2020, 11, 5), new Date(2020, 11, 30));
            var vacationTimeRange = new TimeRange(new Date(2020, 11, 25), new Date(2020, 12, 5));

            var rangesToRemove = new List<TimeRange>
            {
                vacationTimeRange
            };

            var ranges = target.RemoveRanges(rangesToRemove);
            Assert.Single(ranges);
            var range = ranges.First();
            Assert.Equal(target.From, range.From);
            Assert.Equal(new Date(vacationTimeRange.From.AddDays(-1)).EndOfTheDay(), range.To);
        }

        [Fact]
        public void RemoveRanges_RangeFullyCover_MainRange_Ok()
        {
            var target = new MonthRange(new Date(2020, 12, 1), new Date(2020, 12, 2));
            var vacationTimeRange = new TimeRange(new Date(2020, 11, 25), new Date(2020, 12, 5));

            var rangesToRemove = new List<TimeRange>
            {
                vacationTimeRange
            };

            var ranges = target.RemoveRanges(rangesToRemove);
            Assert.Empty(ranges);
        }

        [Fact]
        public void RemoveRanges_NoRangesToRemove_Exception()
        {
            var target = new MonthRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            Assert.Throws<InvalidOperationException>(() => target.RemoveRanges(Array.Empty<TimeRange>()));
        }

        [Fact]
        public void RemoveRanges_Null_Exception()
        {
            var target = new MonthRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            Assert.Throws<ArgumentNullException>(() => target.RemoveRanges(null));
        }
    }
}