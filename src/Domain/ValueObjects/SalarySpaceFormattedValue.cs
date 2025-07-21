using System.Globalization;

namespace Domain.ValueObjects;

public record SalarySpaceFormattedValue
{
    private readonly double _source;

    public SalarySpaceFormattedValue(double source)
    {
        _source = source;
    }

    public override string ToString()
    {
        return _source
            .ToString("N0", CultureInfo.InvariantCulture)
            .Replace(",", " ");
    }
}