using System;
using MG.Utils.Abstract.Dates;
using Xunit;

namespace MG.Utils.Abstract.Test.Dates
{
    public class DateTest
    {
        [Theory]
        [InlineData("2020-09-01T00:00:00.00+0{0}:00", "yyyy-MM-ddTHH:mm:ss.ffzzzz")]
        [InlineData("2020-09-01", "yyyy-MM-dd")]
        public void ToFormat_DifferentFormats(string expected, string format)
        {
            var timezone = DateTimeOffset.Now.Offset.Hours;
            expected = string.Format(expected, timezone.ToString());

            Assert.Equal(
                expected,
                new Date(2020, 9, 1).ToString(format));
        }

        [Fact]
        public void ToJiraIso_Ok()
        {
            Assert.Equal(
                $"2020-09-01T00:00:00.00+0{DateTimeOffset.Now.Offset.Hours}:00",
                new Date(2020, 9, 1).ToJiraIso());
        }

        [Fact]
        public void Today_Ok()
        {
            Assert.Equal(
                new DateTimeOffset(DateTime.Today),
                Date.Today.Source);
        }

        [Fact]
        public void Yesterday_Ok()
        {
            Assert.Equal(
                new DateTimeOffset(DateTime.Today).AddDays(-1),
                Date.Yesterday.Source);
        }

        [Fact]
        public void Tomorrow_Ok()
        {
            Assert.Equal(
                new DateTimeOffset(DateTime.Today).AddDays(1),
                Date.Tomorrow.Source);
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(2, 1, false)]
        [InlineData(1, 2, true)]
        [InlineData(31, 12, false)]
        public void IsFirstDayOfMonth_DifferentDays_True(int day, int month, bool expected)
        {
            Assert.Equal(
                expected: expected,
                actual: new Date(2020, month, day).IsFirstDayOfMonth());
        }

        [Theory]
        [InlineData(1, 1, false)]
        [InlineData(2, 1, false)]
        [InlineData(1, 2, false)]
        [InlineData(31, 12, true)]
        [InlineData(31, 1, true)]
        [InlineData(29, 2, true)] // 2020.02.29 exists
        public void IsLastDayOfMonth_DifferentDays_True(int day, int month, bool expected)
        {
            Assert.Equal(
                expected: expected,
                actual: new Date(2020, month, day).IsLastDayOfMonth());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(28)]
        [InlineData(29)]
        public void FirstDayOfMonth_Ok(int day)
        {
            Assert.Equal(
                expected: DateOffset(2020, 2, 1),
                actual: new Date(2020, 2, day).FirstDayOfMonth().StartOfTheDay());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(28)]
        [InlineData(29)]
        public void LastDayOfMonth_Ok(int day)
        {
            Assert.Equal(
                expected: DateOffset(2020, 2, 29, 23, 59, 59),
                actual: new Date(2020, 2, day).LastDayOfMonth().EndOfTheDay());
        }

        [Theory]
        [InlineData(2, 3, 4)]
        [InlineData(0, 0, 4)]
        [InlineData(23, 59, 58)]
        public void StartOfTheDay_Ok(int hour, int minute, int sec)
        {
            Assert.Equal(
                expected: DateOffset(2020, 6, 12),
                actual: new Date(
                    DateOffset(2020, 6, 12, hour, minute, sec)).StartOfTheDay());
        }

        [Theory]
        [InlineData(2, 3, 4)]
        [InlineData(0, 0, 4)]
        [InlineData(23, 59, 58)]
        public void EndOfTheDay_Ok(int hour, int minute, int sec)
        {
            Assert.Equal(
                expected: DateOffset(2020, 6, 12, 23, 59, 59),
                actual: new Date(
                    DateOffset(2020, 6, 12, hour, minute, sec)).EndOfTheDay());
        }

        [Fact]
        public void PreviousWeekStartDate_Ok()
        {
            Assert.Equal(
                  new Date(DateTime.Today).AddDays(-6),
                  Date.Today.PreviousWeekStartDate());

            Assert.Equal(
                  new Date(DateOffset(2020, 6, 12)).AddDays(-6),
                  new Date(
                      DateOffset(2020, 6, 12)).PreviousWeekStartDate());
        }

        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(0)]
        public void WithTimezone_DefinedTimezone(int offset)
        {
            var target = Date.Tomorrow;

            Assert.Equal(offset, target.WithTimezone(offset).Source.Offset.Hours);
            Assert.Equal(offset, target.WithTimezone(offset).Source.Offset.Hours);
        }

        private DateTimeOffset DateOffset(int year, int month, int day, int hour = 0, int minute = 0, int sec = 0)
        {
            return new DateTimeOffset(new DateTime(year, month, day, hour, minute, sec));
        }
    }
}