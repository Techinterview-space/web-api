using System;
using Domain.Totp;
using Xunit;

namespace Domain.Tests.Totp;

public class TotpCodeTests
{
    [Fact]
    public void ToString_Cases_Match()
    {
        var secretKey = TotpSecretKey.Random();
        var now = DateTime.UtcNow;

        var result1 = new TotpCode(secretKey, now).ToString();
        Assert.NotNull(result1);

        var result2 = new TotpCode(secretKey, now).ToString();
        Assert.NotNull(result2);

        Assert.Equal(result1, result2);
    }
}