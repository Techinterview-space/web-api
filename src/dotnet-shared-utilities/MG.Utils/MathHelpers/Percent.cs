using System;
using MG.Utils.Abstract.Extensions;

namespace MG.Utils.MathHelpers
{
    public record Percent
    {
        public const int Hundred = 100;

        public const int Fifty = 50;

        public const int Ten = 10;

        public const int Zero = 0;

        public double Value { get; }

        public Percent(double value)
        {
            if (value < Zero || value > Hundred)
            {
                throw new ArgumentException($"The value '{value}' could not be used as Percent value", nameof(value));
            }

            Value = value;
        }

        public Percent(double first, double second)
        {
            if (second.EqualTo(Zero))
            {
                throw new ArgumentException("Zero division", nameof(second));
            }

            Value = (first / second) * Hundred;
        }

        /// <summary>
        /// Returns percent value as fraction. [0: 1].
        /// </summary>
        /// <returns>A fraction.</returns>
        public double AsFraction() => Value / Hundred;
    }
}