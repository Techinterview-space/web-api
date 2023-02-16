using System;
using System.Linq;
using System.Security.Cryptography;

namespace MG.Utils.Abstract.NonNullableObjects
{
    public record HashedString
    {
        private readonly string _hashedString;

        public HashedString(string hashedString)
        {
            _hashedString = hashedString ?? throw new ArgumentNullException(nameof(hashedString));
        }

        public bool Same(string anotherPassword)
        {
            return Check(anotherPassword).Verified;
        }

        internal (bool Verified, bool NeedsUpgrade) Check(string password)
        {
            var parts = _hashedString.Split('.', 3);

            if (parts.Length != 3)
            {
                throw new FormatException("Unexpected hash format. Should be formatted as `{iterations}.{salt}.{hash}`");
            }

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var needsUpgrade = iterations != HashString.Iterations;

            using var algorithm = new Rfc2898DeriveBytes(
                password: password,
                salt: salt,
                iterations: iterations,
                hashAlgorithm: HashAlgorithmName.SHA512);

            var keyToCheck = algorithm.GetBytes(HashString.KeySize);

            var verified = keyToCheck.SequenceEqual(key);

            return (verified, needsUpgrade);
        }
    }
}