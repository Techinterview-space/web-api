using System;

namespace Domain.Entities.Github;

public class GithubPersonalUserToken : HasDatesBase
{
    public Guid Id { get; protected set; }

    public string Token { get; protected set; }

    public DateTime ExpiresAt { get; protected set; }

    public GithubPersonalUserToken(
        string token,
        DateTime expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
    }

    protected GithubPersonalUserToken()
    {
    }
}