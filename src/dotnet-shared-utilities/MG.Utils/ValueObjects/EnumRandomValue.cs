using System;

namespace MG.Utils.ValueObjects
{
    public static class EnumRandomValue
    {
        public static TEnum Get<TEnum>()
            where TEnum : Enum
        {
            Array values = Enum.GetValues(typeof(TEnum));
            var random = new System.Random();

            // avoiding default value
            return (TEnum)values.GetValue(random.Next(1, values.Length));
        }
    }
}