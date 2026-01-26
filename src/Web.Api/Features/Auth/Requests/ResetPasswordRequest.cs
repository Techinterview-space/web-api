using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.Auth.Requests;

public record ResetPasswordRequest
{
    [Required]
    public string Token { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; init; }
}
