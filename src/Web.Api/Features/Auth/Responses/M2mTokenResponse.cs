using System.Text.Json.Serialization;

namespace Web.Api.Features.Auth.Responses;

public record M2mTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("scopes")]
    public string[] Scopes { get; init; }
}
