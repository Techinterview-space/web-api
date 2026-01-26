using System;

namespace Domain.ValueObjects;

public record DateTimeRoundedRangeSplitter : DateTimeRangeSplitter
{
    public DateTimeRoundedRangeSplitter(
        DateTime start,
        DateTime end,
        int intervalInMinutes)
        : base(
            RoundDown(start),
            RoundUp(end),
            TimeSpan.FromMinutes(intervalInMinutes))
    {
    }

    private static DateTime RoundDown(
        DateTime date)
    {
        if (date.Minute < 15)
        {
            return date.AddMinutes(-date.Minute).AddSeconds(-date.Second);
        }

        if (date.Minute < 30)
        {
            return date.AddMinutes(15 - date.Minute).AddSeconds(-date.Second);
        }

        if (date.Minute < 45)
        {
            return date.AddMinutes(30 - date.Minute).AddSeconds(-date.Second);
        }

        return date.AddMinutes(45 - date.Minute).AddSeconds(-date.Second);
    }

    private static DateTime RoundUp(
        DateTime date)
    {
        if (date.Minute < 15)
        {
            return date.AddMinutes(15 - date.Minute).AddSeconds(-date.Second);
        }

        if (date.Minute < 30)
        {
            return date.AddMinutes(30 - date.Minute).AddSeconds(-date.Second);
        }

        if (date.Minute < 45)
        {
            return date.AddMinutes(45 - date.Minute).AddSeconds(-date.Second);
        }

        return date.AddMinutes(60 - date.Minute).AddSeconds(-date.Second);
    }
}