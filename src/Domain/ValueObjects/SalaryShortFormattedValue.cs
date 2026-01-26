using System.Globalization;

namespace Domain.ValueObjects;

public record SalaryShortFormattedValue
{
    private readonly double _source;

    public SalaryShortFormattedValue(double source)
    {
        _source = source;
    }

    public override string ToString()
    {
        if (_source < 1000)
        {
            return _source.ToString("N0", CultureInfo.InvariantCulture);
        }

        if (_source < 1_000_000)
        {
            return $"{(_source / 1000).ToString("N0", CultureInfo.InvariantCulture)}к";
        }

        if (_source < 1_000_000_000)
        {
            var value = _source / 1_000_000;
            var format = value % 1 != 0 ? "N1" : "N0";

            return $"{(_source / 1_000_000).ToString(format, CultureInfo.InvariantCulture)}млн";
        }

        return _source.ToString("N0", CultureInfo.InvariantCulture);
    }
}