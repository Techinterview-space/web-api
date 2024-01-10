using System;

namespace Domain.ValueObjects.Dates;

public record DateQuarter
{
    public static DateQuarter Current => new (DateTimeOffset.UtcNow);

    public int Year { get; }

    public int Quarter { get; }

    private readonly DateTimeOffset _date;

    public DateQuarter(
        DateTimeOffset date)
    {
        _date = date;
        Year = _date.Year;
        Quarter = ((_date.Month - 1) / 3) + 1;
    }

    public override string ToString()
    {
        return $"Q{Quarter}.{Year}";
    }
}