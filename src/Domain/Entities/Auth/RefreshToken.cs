using System;
using Domain.Entities.Users;

namespace Domain.Entities.Auth;

public class RefreshToken : BaseModel
{
    protected RefreshToken()
    {
    }

    public RefreshToken(
        long userId,
        string token,
        DateTimeOffset expiresAt,
        string deviceInfo = null)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        DeviceInfo = deviceInfo;
        IsRevoked = false;
    }

    public string Token { get; protected set; }

    public long UserId { get; protected set; }

    public virtual User User { get; protected set; }

    public DateTimeOffset ExpiresAt { get; protected set; }

    public bool IsRevoked { get; protected set; }

    public string DeviceInfo { get; protected set; }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public bool IsValid()
    {
        return !IsRevoked && ExpiresAt > DateTimeOffset.UtcNow;
    }
}
