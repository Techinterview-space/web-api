using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Validation;

namespace Domain.ValueObjects.Dates
{
    /// <summary>
    /// Represents a range of time between two dates: <see cref="From"/> and <see cref="To"/>.
    /// <see cref="From"/> is always earlier than <see cref="To"/>. Otherwise, an exception will be thrown.
    /// </summary>
    public class TimeRange
    {
        public static DateTimeOffset Max { get; } = new Date(2100, 12, 31).EndOfTheDay();

        public static DateTimeOffset Min { get; } = new Date(2000, 1, 1).StartOfTheDay();

        private readonly Date _from;

        private readonly Date _to;

        /// <summary>
        /// Gets start of the range.
        /// </summary>
        public DateTimeOffset From { get; }

        /// <summary>
        /// Gets finish date of the range.
        /// </summary>
        public DateTimeOffset To { get; }

        public TimeRange(Date date)
            : this(date, date)
        {
        }

        public TimeRange(Date from, Date to)
        {
            from.ThrowIfNull(nameof(from));
            to.ThrowIfNull(nameof(to));

            _from = from;
            _to = to;

            if (_from.Later(_to))
            {
                throw new InvalidOperationException($"'{nameof(From)}' should not be later than '{nameof(To)}'");
            }

            From = _from.StartOfTheDay();
            To = _to.EndOfTheDay();

            if (From.Earlier(Min) || From.Later(Max))
            {
                throw new InvalidOperationException($"The 'From':{From} is invalid date");
            }

            if (To.Earlier(Min) || To.Later(Max))
            {
                throw new InvalidOperationException($"The 'To':{To} is invalid date");
            }
        }

        public TimeRange(DateTimeOffset @from, DateTimeOffset? to)
            : this(new Date(@from), new Date(to ?? @from))
        {
        }

        public Date Start() => _from;

        public Date End() => _to;

        /// <summary>
        /// Returns a collection of <see cref="TimeRange"/> where
        /// every item represents each day between source From and To dates range.
        /// From is equal to start of the single day and To is equal to end of the day.
        /// </summary>
        /// <returns>Collection of Time Ranges.</returns>
        public IReadOnlyCollection<DayRange> SplitByDays()
        {
            var list = new List<DayRange>();

            if (_from.Source.SameDay(_to.Source))
            {
                list.Add(new DayRange(_from));
                return list;
            }

            var from = _from.StartOfTheDay();
            var to = _to.EndOfTheDay();

            for (DateTimeOffset currentDay = from; currentDay <= to; currentDay = currentDay.AddDays(1))
            {
                list.Add(new DayRange(new Date(currentDay)));
            }

            return list;
        }

        /// <summary>
        /// Returns a collection of <see cref="TimeRange"/> where
        /// From is a start of the month (or <see cref="From"/>) and To is the end of the month (or <see cref="To"/>).
        /// At least one time range will be returned.
        /// </summary>
        /// <returns>Collection of Time Ranges.</returns>
        public IReadOnlyCollection<MonthRange> SplitByMonths()
        {
            var list = new List<MonthRange>();

            if (SameMonth())
            {
                list.Add(new MonthRange(from: _from.StartOfTheDay(), to: _to.EndOfTheDay()));
                return list;
            }

            DateTimeOffset firstDayOfTheLastTimelineMonth = _to.FirstDayOfMonth().StartOfTheDay();

            // Add first month remaining days
            list.Add(new MonthRange(_from.StartOfTheDay(), _from.LastDayOfMonth().EndOfTheDay()));

            // Add All months days in between
            for (var st = From.AddMonths(1); st < firstDayOfTheLastTimelineMonth; st = st.AddMonths(1))
            {
                var extended = new Date(st);
                list.Add(new MonthRange(extended.FirstDayOfMonth(), extended.LastDayOfMonth()));
            }

            // Add last month days
            list.Add(new MonthRange(firstDayOfTheLastTimelineMonth, _to.EndOfTheDay()));

            return list;
        }

        public bool SameMonth()
        {
            return _from.SameMonth(_to);
        }

        public bool FullMonth()
        {
            if (!SameMonth())
            {
                return false;
            }

            return _from.IsFirstDayOfMonth() && _to.IsLastDayOfMonth();
        }

        public bool Contains(TimeRange range)
        {
            return range._from.Source >= _from.Source &&
                   range._to.Source <= _to.Source;
        }

        // TODO Maxim: unittests
        public bool Contains(Date date)
        {
            return date.Source >= _from.Source &&
                date.Source <= _to.Source;
        }

        public TimeRange IntersectionOrNull(TimeRange second)
        {
            if (this.Contains(second))
            {
                return second;
            }

            if (second.Contains(this))
            {
                return this;
            }

            if (second.From.EarlierOrEqual(_from.Source) &&
                second.To.LaterOrEqual(_from.Source) &&
                second.To.EarlierOrEqual(_to.Source))
            {
                return new TimeRange(_from.Clone(), new Date(second.To));
            }

            if (second.From.LaterOrEqual(_from.Source) &&
                second.From.EarlierOrEqual(_to.Source) &&
                second.To.LaterOrEqual(_to.Source))
            {
                return new TimeRange(new Date(second.From), _to.Clone());
            }

            return null;
        }

        public virtual IReadOnlyCollection<TimeRange> RemoveRanges(IReadOnlyCollection<TimeRange> rangesToRemove)
        {
            rangesToRemove.ThrowIfNullOrEmpty(nameof(rangesToRemove));

            IReadOnlyCollection<DayRange> days = SplitByDays();

            DayRange[] daysNotInTheRangesToRemove = days
                .Where(day => rangesToRemove.All(range => !range.Contains(day)))
                .OrderBy(x => x.From)
                .ToArray();

            if (!daysNotInTheRangesToRemove.Any())
            {
                return Array.Empty<TimeRange>();
            }

            var result = new List<TimeRange>();

            DayRange previous, start;
            previous = start = daysNotInTheRangesToRemove.First();

            // index = 1 because we have to skip the first element
            for (var index = 1; index < daysNotInTheRangesToRemove.Length; index++)
            {
                DayRange dayRange = daysNotInTheRangesToRemove[index];

                if (!dayRange.NextFor(previous))
                {
                    result.Add(new TimeRange(
                        start.AsDate(),
                        daysNotInTheRangesToRemove[index - 1].AsDate()));

                    start = dayRange;
                }

                if (index == daysNotInTheRangesToRemove.Length - 1)
                {
                    result.Add(new TimeRange(
                        start.AsDate(),
                        dayRange.AsDate()));
                }

                previous = dayRange;
            }

            return result;
        }

        public IReadOnlyCollection<DayRange> WorkDays()
        {
            var days = SplitByDays();

            return days.Where(x => !x.Weekend()).ToArray();
        }

        public override string ToString()
        {
            return $"{GetType().Name}. [{_from}:{_to}]";
        }

        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as TimeRange);
        }

        // was generated by Resharper
        public override int GetHashCode()
        {
            unchecked
            {
                return (_from.Source.GetHashCode() * 397) ^ _to.Source.GetHashCode();
            }
        }

        public bool Equals(TimeRange range)
        {
            return range != null && From.Equal(range.From) && To.Equal(range.To);
        }

        public IReadOnlyCollection<TimeRange> SplitByWeeks()
        {
            const int weekChunkSize = 6;

            var startOfTheFirstRange = Start();
            if (startOfTheFirstRange.Source.DayOfWeek == DayOfWeek.Monday)
            {
                return SplitByChunks(weekChunkSize, true).ToArray();
            }

            var endOfTheFirstRange = Start().EndOfTheWeek();

            var result = new List<TimeRange>
            {
                new TimeRange(startOfTheFirstRange, endOfTheFirstRange)
            };

            var nextRange = new TimeRange(endOfTheFirstRange.AddDays(1), End());
            result.AddRange(nextRange.SplitByChunks(weekChunkSize, true));

            return result;
        }

        public IEnumerable<TimeRange> SplitByChunks(int dayChunkSize)
        {
            return SplitByChunks(dayChunkSize, false);
        }

        private IEnumerable<TimeRange> SplitByChunks(int dayChunkSize, bool addNextDay)
        {
            Date chunkEnd;
            Date start = Start();
            Date end = End();

            while (end.Later(chunkEnd = start.AddDays(dayChunkSize)))
            {
                yield return new TimeRange(start, chunkEnd);
                start = addNextDay ? chunkEnd.AddDays(1) : chunkEnd;
            }

            yield return new TimeRange(start, end);
        }
    }
}