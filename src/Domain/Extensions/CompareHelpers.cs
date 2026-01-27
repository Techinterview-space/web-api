using System;

namespace Domain.Extensions;

public static class CompareHelpers
{
    private const double Tolerance = 0.01;

    public static bool EqualTo(this double first, double second, double tolerance = Tolerance)
    {
        return Math.Abs(first - second) < tolerance;
    }

    public static bool EqualTo(this double? first, double? second, double tolerance = Tolerance)
    {
        if (first == null && second == null)
        {
            return true;
        }

        if ((first.HasValue && !second.HasValue) || (!first.HasValue && second.HasValue))
        {
            return false;
        }

        return Math.Abs(first.Value - second.Value) < tolerance;
    }
}