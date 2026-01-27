using System;

namespace Domain.ValueObjects;

public record RoundedValuesByRangesSplitter : ValuesByRangesSplitter
{
    public RoundedValuesByRangesSplitter(
        double min,
        double max,
        int step)
        : base(
            RoundDown(min, step),
            RoundUp(max, step),
            step)
    {
    }

    private static double RoundDown(double value, int step)
    {
        return Math.Floor(value / step) * step;
    }

    private static double RoundUp(double value, int step)
    {
        return Math.Ceiling(value / step) * step;
    }
}