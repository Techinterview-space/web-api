using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.Auth.Requests;

public record RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; }
}
