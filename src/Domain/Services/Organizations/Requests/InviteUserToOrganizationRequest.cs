using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Services.Organizations;

public record InviteUserToOrganizationRequest
{
    public Guid OrganizationId { get; init; }

    [Required]
    public string Email { get; init; }
}