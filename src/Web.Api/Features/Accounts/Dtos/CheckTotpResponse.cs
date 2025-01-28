namespace Web.Api.Features.Accounts.Dtos;

public record CheckTotpResponse
{
    public CheckTotpResponse(
        bool result)
    {
        Result = result;
    }

    public bool Result { get; }
}