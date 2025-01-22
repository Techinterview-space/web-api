using System;
using OtpNet;

namespace Domain.Totp;

public record TotpCode
{
    public TotpSecretKey SecretKey { get; }

    public DateTime? Timestamp { get; }

    private readonly int _step;
    private readonly OtpHashMode _hashMode;

    private string _result;

    public TotpCode(
        TotpSecretKey secretKey,
        DateTime? timestamp = null,
        int step = 30,
        OtpHashMode hashMode = OtpHashMode.Sha1)
    {
        if (step < 10)
        {
            throw new ArgumentException("Step must be at least 10.");
        }

        SecretKey = secretKey;
        _step = step;
        _hashMode = hashMode;
        Timestamp = timestamp;
    }

    public override string ToString()
        => _result ??= Generate();

    private string Generate()
    {
        var totp = new OtpNet.Totp(
            SecretKey.KeyAsBytes,
            step: _step,
            _hashMode);

        var totpCode = Timestamp.HasValue
            ? totp.ComputeTotp(Timestamp.Value)
            : totp.ComputeTotp();

        return totpCode;
    }
}