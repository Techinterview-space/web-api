namespace Domain.ValueObjects.OAuth;

public record OAuthUserInfo
{
    public string Id { get; init; }

    public string Email { get; init; }

    public bool EmailVerified { get; init; }

    public string GivenName { get; init; }

    public string FamilyName { get; init; }

    public string Name { get; init; }

    public string Picture { get; init; }
}
