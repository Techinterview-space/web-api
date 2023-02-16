using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MG.Utils.Abstract.NonNullableObjects
{
    public record Sha256String
    {
        public Sha256String(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            using var hashstring = SHA512.Create();

            byte[] hash = hashstring.ComputeHash(
                Encoding.UTF8.GetBytes(value));

            Value = hash
                .Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
        }

        public string Value { get; }

        public static explicit operator string(Sha256String sha)
        {
            return sha?.Value;
        }
    }
}