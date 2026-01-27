namespace Web.Api.Features.Accounts.Dtos;

public record CheckTotpRequest
{
    public string TotpCode { get; init; }
}