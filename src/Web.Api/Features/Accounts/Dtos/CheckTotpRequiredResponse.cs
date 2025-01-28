namespace Web.Api.Features.Accounts.Dtos;

public record CheckTotpRequiredResponse
{
    public long Id { get; init; }

    public string Email { get; init; }

    public bool IsMfaEnabled { get; init; }
}