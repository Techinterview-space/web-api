namespace Web.Api.Features.Auth.Requests;

public record LogoutRequest
{
    public string RefreshToken { get; init; }
}
