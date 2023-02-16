using System;
using System.Security.Cryptography;
using MG.Utils.Abstract.Random;

namespace MG.Utils.Abstract.NonNullableObjects
{
    public record HashString
    {
        public static HashString Random(int length = 24)
        {
            return new ((string)new RandomString(
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", length));
        }

        internal const int SaltSize = 16; // 128 bit
        internal const int KeySize = 32; // 256 bit
        internal const int Iterations = 1000;

        private readonly string _source;
        private string _value;

        public HashString(string source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public string Value
        {
            get
            {
                return _value ??= ValueInternal();
            }
        }

        private string ValueInternal()
        {
            using var algorithm = new Rfc2898DeriveBytes(
                password: _source,
                saltSize: SaltSize,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA512);

            var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
            var salt = Convert.ToBase64String(algorithm.Salt);

            return $"{Iterations}.{salt}.{key}";
        }

        public static explicit operator string(HashString pass)
        {
            return pass?.Value;
        }
    }
}