using System;

namespace MG.Utils.Abstract.NonNullableObjects
{
    public record RandomToken
    {
        public string Value { get; }

        public RandomToken()
        {
            Value = (string)new Sha256String(
                        DateTimeOffset.Now.Ticks +
                        Guid.NewGuid()
                            .ToString()
                            .Replace("-", string.Empty));
        }

        public static explicit operator string(RandomToken instance)
        {
            return instance?.Value;
        }
    }
}