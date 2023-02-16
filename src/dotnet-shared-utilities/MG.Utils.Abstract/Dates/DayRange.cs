using System;

namespace MG.Utils.Abstract.Dates
{
    /// <summary>
    /// Represents Time Range between start and end of the one single day.
    /// </summary>
    public class DayRange : TimeRange
    {
        /// <summary>
        /// Gets month that the day belongs to.
        /// </summary>
        public int Month => _source.Month;

        /// <summary>
        /// Gets year that the day belongs to.
        /// </summary>
        public int Year => _source.Year;

        /// <summary>
        /// Gets day itself as int value.
        /// </summary>
        public int Day => _source.Day;

        private readonly Date _source;

        public DayRange(
            DateTimeOffset source)
            : this(new Date(source))
        {
        }

        public DayRange(
            Date source)
            : base(source)
        {
            _source = source;
        }

        /// <summary>
        /// Returns true if the day is weekend.
        /// </summary>
        /// <returns>True if weekend.</returns>
        public bool Weekend() => _source.Weekend();

        public Date AsDate() => _source;

        public bool NextFor(DayRange second)
        {
            second.ThrowIfNull(nameof(second));

            return this.AsDate().SameDay(second.AsDate().AddDays(1));
        }
    }
}