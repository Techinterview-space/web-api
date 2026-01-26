using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.Auth.Requests;

public record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; }
}
