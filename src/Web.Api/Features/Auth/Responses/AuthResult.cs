namespace Web.Api.Features.Auth.Responses;

public record AuthResult
{
    public bool Success { get; init; }

    public string Message { get; init; }
}
