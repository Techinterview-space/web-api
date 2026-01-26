using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.Auth.Requests;

public record RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; }

    [Required]
    [StringLength(150)]
    public string FirstName { get; init; }

    [Required]
    [StringLength(150)]
    public string LastName { get; init; }
}
