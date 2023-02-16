using System;
using MG.Utils.Abstract;
using MG.Utils.Abstract.Dates;
using MG.Utils.Abstract.Dates.Interfaces;
using MG.Utils.Abstract.Entities;
using MG.Utils.Exceptions;

namespace MG.Utils.Helpers
{
    public static class TimeRangeExtensions
    {
        public static bool Active(this IHasFromToDates source)
        {
            source.ThrowIfNull(nameof(source));

            var today = Date.Today.StartOfTheDay();
            return (source.From.Earlier(today) || source.From.SameDay(today)) && source.To.Later(today);
        }

        public static bool ComingSoon(this IHasFromToDates source)
        {
            source.ThrowIfNull(nameof(source));

            var tomorrow = Date.Tomorrow.StartOfTheDay();
            return source.From.SameDay(tomorrow) || source.From.Later(tomorrow);
        }

        public static bool Inactive(this IHasFromToDates source)
        {
            source.ThrowIfNull(nameof(source));

            DateTimeOffset yesterday = Date.Yesterday.EndOfTheDay();
            DateTimeOffset to = source.ToOrFail();
            return to.Earlier(yesterday) || to.SameDay(yesterday);
        }

        public static void ActiveOrFail<T>(this T source)
            where T : IHasFromToDates, IHasId
        {
            source.ThrowIfNull(nameof(source));

            if (source.Inactive())
            {
                throw new BadRequestException($"The {typeof(T).Name}:{source.Id} is not active or coming soon");
            }
        }
    }
}