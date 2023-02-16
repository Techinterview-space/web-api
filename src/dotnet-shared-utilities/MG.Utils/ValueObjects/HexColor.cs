using System;

namespace MG.Utils.ValueObjects;

public class HexColor
{
    public static HexColor Random()
    {
        var random = new Random();
        return new HexColor($"#{random.Next(0x1000000):X6}");
    }

    private readonly string _hexColor;

    public HexColor(string hexColor)
    {
        hexColor = hexColor?.Trim();
        if (string.IsNullOrEmpty(hexColor))
        {
            throw new ArgumentNullException(nameof(hexColor));
        }

        if (hexColor.Length is < 6 or > 7)
        {
            throw new ArgumentException("Hex color must be 6 or 7 characters long.", nameof(hexColor));
        }

        _hexColor = hexColor.StartsWith("#") ? hexColor : hexColor.Insert(0, "#");
    }

    public override string ToString() => _hexColor;

    private bool Equals(HexColor other)
        => _hexColor == other._hexColor;

    public override bool Equals(object obj)
        => ReferenceEquals(this, obj) ||
           (obj is HexColor other && Equals(other));

    public override int GetHashCode()
        => _hexColor.GetHashCode();
}