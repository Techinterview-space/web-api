using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ValueObjects.Dates;
using Xunit;
using Date = Domain.ValueObjects.Dates.Date;
using TimeRange = Domain.ValueObjects.Dates.TimeRange;

namespace DomainTests.ValueObjects.Dates
{
    public class TimeRangeTest
    {
        [Theory]
        [InlineData(1, 31)]
        [InlineData(2, 30)]
        [InlineData(10, 20)]
        [InlineData(15, 15)]
        public void SameMonth_ReallySingleMonth_True(int firstDay, int lastDay)
        {
            Assert.True(new TimeRange(
                new Date(2020, 8, firstDay), new Date(2020, 8, lastDay)).SameMonth());
        }

        [Fact]
        public void SameMonth_NotSameMonthOrYear_False()
        {
            Assert.False(new TimeRange(
                new Date(2020, 7, 1), new Date(2020, 8, 31)).SameMonth());

            Assert.False(new TimeRange(
                new Date(2020, 8, 1), new Date(2021, 8, 31)).SameMonth());
        }

        [Fact]
        public void FullMonth_ReallyFullMonthDates_True()
        {
            Assert.True(new TimeRange(
                new Date(2020, 8, 1), new Date(2020, 8, 31)).FullMonth());
        }

        [Fact]
        public void FullMonth_DifferentMonths_False()
        {
            Assert.False(new TimeRange(
                new Date(2020, 7, 1), new Date(2020, 8, 31)).FullMonth());
        }

        [Fact]
        public void FullMonth_NotFirstDateOfMonth_False()
        {
            Assert.False(new TimeRange(
                new Date(2020, 8, 2), new Date(2020, 8, 31)).FullMonth());

            Assert.False(new TimeRange(
                new Date(2020, 8, 1), new Date(2020, 8, 30)).FullMonth());

            Assert.False(new TimeRange(
                new Date(2020, 8, 2), new Date(2020, 8, 30)).FullMonth());
        }

        [Theory]
        [InlineData(-10, 10)]
        [InlineData(-9, 9)]
        [InlineData(0, 0)]
        public void Contains_TheSecondIsIncluded_True(int fromDays, int toDays)
        {
            Assert.True(
                new TimeRange(Date.Yesterday.AddDays(-10), Date.Tomorrow.AddDays(10)).Contains(
                    new TimeRange(Date.Yesterday.AddDays(fromDays), Date.Tomorrow.AddDays(toDays))));
        }

        [Fact]
        public void Contains_TheSecondIsNotIncluded_OnlyOneDateIsNotIncluded_False()
        {
            Assert.False(
                new TimeRange(Date.Yesterday.AddDays(-10), Date.Tomorrow.AddDays(10)).Contains(
                    new TimeRange(Date.Yesterday.AddDays(-11), Date.Tomorrow.AddDays(10))));

            Assert.False(
                new TimeRange(Date.Yesterday.AddDays(-10), Date.Tomorrow.AddDays(10)).Contains(
                    new TimeRange(Date.Yesterday.AddDays(-10), Date.Tomorrow.AddDays(11))));
        }

        [Fact]
        public void Contains_TheSecondIsNotIncluded_OutOfRange_False()
        {
            Assert.False(
                new TimeRange(Date.Yesterday.AddDays(-10), Date.Tomorrow.AddDays(10)).Contains(
                    new TimeRange(Date.Yesterday.AddDays(-11), Date.Tomorrow.AddDays(11))));
        }

        [Fact]
        public void Ctor_FromLaterThanTo_Exception()
        {
            var from = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(2));
            var to = from.Subtract(TimeSpan.FromDays(5));

            Assert.Throws<InvalidOperationException>(() => new TimeRange(from, to));
        }

        [Fact]
        public void From_NowPassed_StartOfTheDayReturns()
        {
            DateTimeOffset from = new TimeRange(Date.Now).From;

            Assert.Equal(
                new DateTimeOffset(DateTime.Today),
                from);

            Assert.Equal(0, from.Hour);
            Assert.Equal(0, from.Minute);
            Assert.Equal(0, from.Second);
        }

        [Fact]
        public void To_NowPassed_EndOfTheDayReturns()
        {
            var now = Date.Now;
            var target = new TimeRange(Date.Now);

            Assert.Equal(now.Year, target.To.Year);
            Assert.Equal(now.Month, target.To.Month);
            Assert.Equal(now.Day, target.To.Day);

            Assert.Equal(23, target.To.Hour);
            Assert.Equal(59, target.To.Minute);
            Assert.Equal(59, target.To.Second);
        }

        [Fact]
        public void Ctor_NoToValue_FromIsCopied()
        {
            var today = Date.Today.AddDays(-2);
            var target = new TimeRange(today);

            Assert.NotEqual(default(DateTimeOffset), target.To);
            Assert.Equal(today.StartOfTheDay(), target.From);
            Assert.Equal(today.EndOfTheDay(), target.To);

            Assert.NotEqual(target.From, target.To);
        }

        [Fact]
        public void Ctor_ToValue_Ok()
        {
            Date today = Date.Today.AddDays(-2);
            var target = new TimeRange(today, today.AddDays(5));

            Assert.Equal(today.StartOfTheDay(), target.From);
            Assert.Equal(today.AddDays(5).EndOfTheDay(), target.To);
        }

        [Fact]
        public void Ctor_FromIsEarlierThanMin_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new TimeRange(TimeRange.Min.AddDays(-1), TimeRange.Min.AddDays(1)));
        }

        [Fact]
        public void Ctor_FromIsLaterThanMax_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new TimeRange(TimeRange.Max.AddDays(1), TimeRange.Max.AddDays(2)));
        }

        [Fact]
        public void Ctor_ToIsEarlierThanMin_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new TimeRange(TimeRange.Min.AddDays(-2), TimeRange.Min.AddDays(-1)));
        }

        [Fact]
        public void Ctor_ToIsLaterThanMax_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new TimeRange(TimeRange.Max.AddDays(0), TimeRange.Max.AddDays(1)));
        }

        [Fact]
        public void TimeRangesByMonths_OnlyOneMonth_Ok()
        {
            var first = new Date(2020, 5, 1).StartOfTheDay();
            var second = new Date(2020, 5, 15).EndOfTheDay();

            var target = new TimeRange(first, second);

            IReadOnlyCollection<MonthRange> timeRanges = target.SplitByMonths();

            Assert.NotNull(timeRanges);
            Assert.NotEmpty(timeRanges);

            Assert.Single(timeRanges);

            var timeline = timeRanges.First();
            Assert.Equal(first, timeline.From);
            Assert.Equal(new Date(2020, 5, 15).EndOfTheDay(), timeline.To);
        }

        [Fact]
        public void TimeRangesByMonths_TwoMonths_Ok()
        {
            var first = new Date(2020, 4, 1).StartOfTheDay();
            var second = new Date(2020, 5, 31).EndOfTheDay();

            var target = new TimeRange(first, second);

            IReadOnlyCollection<MonthRange> timeRanges = target.SplitByMonths();

            Assert.NotNull(timeRanges);
            Assert.NotEmpty(timeRanges);

            Assert.Equal(2, timeRanges.Count);

            var firstTimeLine = timeRanges.ElementAt(0);
            var secondTimeLine = timeRanges.ElementAt(1);

            Assert.Equal(first, firstTimeLine.From);
            Assert.Equal(
                new Date(2020, 4, 30).EndOfTheDay(),
                firstTimeLine.To);

            Assert.Equal(new Date(2020, 5, 1).StartOfTheDay(), secondTimeLine.From);
            Assert.Equal(new Date(second).EndOfTheDay(), secondTimeLine.To);
        }

        [Fact]
        public void TimeRangesByMonths_ThreeMonths_Ok()
        {
            var first = new Date(2020, 3, 15).StartOfTheDay();
            var second = new Date(2020, 5, 15).EndOfTheDay();

            var target = new TimeRange(first, second);

            IReadOnlyCollection<MonthRange> timeRanges = target.SplitByMonths();

            Assert.NotNull(timeRanges);
            Assert.NotEmpty(timeRanges);

            Assert.Equal(3, timeRanges.Count);

            var firstTimeLine = timeRanges.ElementAt(0);
            var secondTimeLine = timeRanges.ElementAt(1);
            var thirdTimeLine = timeRanges.ElementAt(2);

            Assert.Equal(first, firstTimeLine.From);
            Assert.Equal(new Date(2020, 3, 31).EndOfTheDay(), firstTimeLine.To);

            Assert.Equal(new Date(2020, 4, 1).StartOfTheDay(), secondTimeLine.From);
            Assert.Equal(new Date(2020, 4, 30).EndOfTheDay(), secondTimeLine.To);

            Assert.Equal(new Date(2020, 5, 1).Source, thirdTimeLine.From);
            Assert.Equal(new Date(second).EndOfTheDay(), thirdTimeLine.To);
        }

        [Fact]
        public void SplitByDays_SameMonth_Ok()
        {
            var target = new TimeRange(
                new Date(2020, 6, 9),
                new Date(2020, 6, 11));
            IReadOnlyCollection<TimeRange> days = target.SplitByDays();

            var expected = new List<TimeRange>
            {
                new DayRange(
                    new Date(2020, 6, 9)),
                new DayRange(
                    new Date(2020, 6, 10)),
                new DayRange(
                    new Date(2020, 6, 11)),
            };

            AssertCollections(expected, days);
        }

        [Fact]
        public void Equals_TimeRangeAndDateRange_Ok()
        {
            var date = new Date(2020, 5, 30);

            Assert.True(new DayRange(date).Equals(new TimeRange(date)));
            Assert.True(new TimeRange(date).Equals(new DayRange(date)));

            Assert.True(new TimeRange(date).Equals(new TimeRange(date)));
            Assert.True(new DayRange(date).Equals(new DayRange(date)));

            Assert.Equal(new DayRange(date), new TimeRange(date));
        }

        [Fact]
        public void SplitByDays_DifferentMonth_Ok()
        {
            var target = new TimeRange(
                new Date(2020, 5, 30),
                new Date(2020, 6, 2));
            IReadOnlyCollection<TimeRange> days = target.SplitByDays();

            var expected = new List<TimeRange>
            {
                new DayRange(
                    new Date(2020, 5, 30)),
                new DayRange(
                    new Date(2020, 5, 31)),
                new DayRange(
                    new Date(2020, 6, 1)),
                new DayRange(
                    new Date(2020, 6, 2)),
            };

            AssertCollections(expected, days);
        }

        private void AssertCollections<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var count = actual.Count();
            Assert.Equal(expected.Count(), count);
            for (int i = 0; i < count; i++)
            {
                Assert.Equal(expected.ElementAt(i), actual.ElementAt(i));
            }
        }

        [Fact]
        public void IntersectionOrNull_RangeContainsSecond_Ok()
        {
            var first = new TimeRange(
                new Date(2020, 6, 1),
                new Date(2020, 6, 30));

            var second = new TimeRange(
                from: new Date(2020, 6, 5).StartOfTheDay(),
                to: new Date(2020, 6, 20).EndOfTheDay());

            TimeRange intersection = first.IntersectionOrNull(second);

            Assert.NotNull(intersection);
            Assert.Equal(new Date(2020, 6, 5).StartOfTheDay(), intersection.From);
            Assert.Equal(
                new Date(2020, 6, 20).EndOfTheDay(),
                intersection.To);
        }

        [Fact]
        public void IntersectionOrNull_SecondContainsTheFirstOne_Ok()
        {
            var first = new TimeRange(
                new Date(2020, 6, 1).StartOfTheDay(),
                new Date(2020, 6, 30).EndOfTheDay());

            var second = new TimeRange(
                from: new Date(2020, 5, 5).StartOfTheDay(),
                to: new Date(2020, 7, 20).EndOfTheDay());

            TimeRange intersection = first.IntersectionOrNull(second);

            Assert.NotNull(intersection);
            Assert.Equal(new Date(2020, 6, 1).StartOfTheDay(), intersection.From);
            Assert.Equal(
                new Date(2020, 6, 30).EndOfTheDay(),
                intersection.To);
        }

        [Fact]
        public void IntersectionOrNull_SecondStartsEarlierThanTheFirstOne_Ok()
        {
            var first = new TimeRange(
                new Date(2020, 6, 1).StartOfTheDay(),
                new Date(2020, 6, 30).EndOfTheDay());

            var second = new TimeRange(
                from: new Date(2020, 5, 5).StartOfTheDay(),
                to: new Date(2020, 6, 20).EndOfTheDay());

            TimeRange intersection = first.IntersectionOrNull(second);

            Assert.NotNull(intersection);
            Assert.Equal(new Date(2020, 6, 1).StartOfTheDay(), intersection.From);
            Assert.Equal(
                new Date(2020, 6, 20).EndOfTheDay(),
                intersection.To);
        }

        [Fact]
        public void IntersectionOrNull_SecondFinishesLaterThanTheFirstOne_Ok()
        {
            var first = new TimeRange(
                new Date(2020, 6, 1).StartOfTheDay(),
                new Date(2020, 6, 30).EndOfTheDay());

            var second = new TimeRange(
                from: new Date(2020, 6, 15).StartOfTheDay(),
                to: new Date(2020, 8, 20).EndOfTheDay());

            TimeRange intersection = first.IntersectionOrNull(second);

            Assert.NotNull(intersection);
            Assert.Equal(new Date(2020, 6, 15).StartOfTheDay(), intersection.From);
            Assert.Equal(
                new Date(2020, 6, 30).EndOfTheDay(),
                intersection.To);
        }

        [Fact]
        public void IntersectionOrNull_NoIntersections_ReturnsNull_Ok()
        {
            var first = new TimeRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            var second = new TimeRange(new Date(2020, 4, 15), new Date(2020, 5, 20));

            TimeRange intersection = first.IntersectionOrNull(second);

            Assert.Null(intersection);
        }

        [Fact]
        public void RemoveRanges_AllRangesIncludedIntoMainOne_Case1_Ok()
        {
            var target = new MonthRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            var rangesToRemove = new List<TimeRange>
            {
                new TimeRange(new Date(2020, 6, 1), new Date(2020, 6, 10))
            };

            IReadOnlyCollection<TimeRange> result = target.RemoveRanges(rangesToRemove);

            Assert.Single(result);

            var first = result.First();

            Assert.True(new Date(2020, 6, 11).SameDay(first.From));
            Assert.True(new Date(2020, 6, 30).SameDay(first.To));
        }

        [Fact]
        public void RemoveRanges_AllRangesIncludedIntoMainOne_Case2_Ok()
        {
            var target = new MonthRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            var rangesToRemove = new List<TimeRange>
            {
                new TimeRange(new Date(2020, 6, 1), new Date(2020, 6, 10)),
                new TimeRange(new Date(2020, 6, 15), new Date(2020, 6, 29))
            };

            IReadOnlyCollection<TimeRange> result = target.RemoveRanges(rangesToRemove);

            Assert.Equal(2, result.Count);

            var first = result.First();
            var last = result.Last();

            Assert.True(new Date(2020, 6, 11).SameDay(first.From));
            Assert.True(new Date(2020, 6, 14).SameDay(first.To));

            Assert.True(new Date(2020, 6, 30).SameDay(last.From));
            Assert.True(new Date(2020, 6, 30).SameDay(last.To));
        }

        [Fact]
        public void RemoveRanges_AllRangesIncludedIntoMainOne_Case3_Ok()
        {
            var target = new MonthRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            var rangesToRemove = new List<TimeRange>
            {
                new TimeRange(new Date(2020, 6, 1), new Date(2020, 6, 1)),
                new TimeRange(new Date(2020, 6, 10), new Date(2020, 6, 20)),
                new TimeRange(new Date(2020, 6, 28), new Date(2020, 6, 30))
            };

            IReadOnlyCollection<TimeRange> result = target.RemoveRanges(rangesToRemove);

            Assert.Equal(2, result.Count);

            Assert.True(new Date(2020, 6, 2).SameDay(result.ElementAt(0).From));
            Assert.True(new Date(2020, 6, 9).SameDay(result.ElementAt(0).To));

            Assert.True(new Date(2020, 6, 21).SameDay(result.ElementAt(1).From));
            Assert.True(new Date(2020, 6, 27).SameDay(result.ElementAt(1).To));
        }

        [Fact]
        public void RemoveRanges_NotAllRangesIncludedIntoMainOne_Case1_Ok()
        {
            var target = new TimeRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            var rangesToRemove = new List<TimeRange>
            {
                new TimeRange(new Date(2020, 6, 15), new Date(2020, 7, 20)),
            };

            IReadOnlyCollection<TimeRange> result = target.RemoveRanges(rangesToRemove);

            Assert.Single(result);

            Assert.True(new Date(2020, 6, 1).SameDay(result.ElementAt(0).From));
            Assert.True(new Date(2020, 6, 14).SameDay(result.ElementAt(0).To));
        }

        [Fact]
        public void RemoveRanges_NotAllRangesIncludedIntoMainOne_Case2_Ok()
        {
            var target = new TimeRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            var rangesToRemove = new List<TimeRange>
            {
                new TimeRange(new Date(2020, 5, 15), new Date(2020, 6, 14)),
            };

            IReadOnlyCollection<TimeRange> result = target.RemoveRanges(rangesToRemove);

            Assert.Single(result);

            Assert.True(new Date(2020, 6, 15).SameDay(result.ElementAt(0).From));
            Assert.True(new Date(2020, 6, 30).SameDay(result.ElementAt(0).To));
        }

        [Fact]
        public void RemoveRanges_NoRangesToRemove_Exception()
        {
            var target = new TimeRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            Assert.Throws<InvalidOperationException>(() => target.RemoveRanges(Array.Empty<TimeRange>()));
        }

        [Fact]
        public void RemoveRanges_Null_Exception()
        {
            var target = new TimeRange(new Date(2020, 6, 1), new Date(2020, 6, 30));

            Assert.Throws<ArgumentNullException>(() => target.RemoveRanges(null));
        }
    }
}