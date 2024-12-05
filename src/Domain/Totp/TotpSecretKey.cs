using System;
using OtpNet;

namespace Domain.Totp;

public record TotpSecretKey
{
    public string KeyAsBase32 { get; }

    public byte[] KeyAsBytes { get; }

    public TotpSecretKey(
        string keyAsBase32)
    {
        KeyAsBytes = Base32Encoding.ToBytes(keyAsBase32);
        KeyAsBase32 = keyAsBase32;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TotpSecretKey"/> class.
    /// </summary>
    /// <param name="keyAsBytes">Bytes as source.</param>
    public TotpSecretKey(
        byte[] keyAsBytes)
    {
        KeyAsBytes = keyAsBytes;
        KeyAsBase32 = Base32Encoding.ToString(keyAsBytes);
    }

    public static TotpSecretKey Random(
        int length = 20)
    {
        if (length < 6)
        {
            throw new ArgumentException("Length must be at least 6.");
        }

        return new TotpSecretKey(
            KeyGeneration.GenerateRandomKey(length));
    }

    public override string ToString()
    {
        return KeyAsBase32;
    }
}