using MG.Utils.Abstract.Dates;
using Xunit;

namespace MG.Utils.Abstract.Test.Dates
{
    public class DayRangeTest
    {
        private Date Date(int year, int month, int day)
        {
            return new Date(year, month, day);
        }

        [Fact]
        public void Ctor_FromToValid_Ok()
        {
            var target = new DayRange(new Date(2020, 6, 12));

            Assert.Equal(Date(2020, 6, 12).StartOfTheDay(), target.From);
            Assert.Equal(Date(2020, 6, 12).EndOfTheDay(), target.To);
        }

        [Fact]
        public void Properties_Ok()
        {
            var target = new DayRange(new Date(2020, 6, 12));

            Assert.Equal(6, target.Month);
            Assert.Equal(2020, target.Year);
        }
    }
}