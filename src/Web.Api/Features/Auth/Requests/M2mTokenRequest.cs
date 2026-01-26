using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Web.Api.Features.Auth.Requests;

public record M2mTokenRequest
{
    [Required]
    [JsonPropertyName("client_id")]
    public string ClientId { get; init; }

    [Required]
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; init; }

    [JsonPropertyName("scopes")]
    public string[] Scopes { get; init; }

    [JsonIgnore]
    public string IpAddress { get; set; }
}
