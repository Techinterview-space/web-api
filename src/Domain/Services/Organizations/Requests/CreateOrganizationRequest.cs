using System.ComponentModel.DataAnnotations;
using Domain.Entities.Organizations;

namespace Domain.Services.Organizations;

public record CreateOrganizationRequest
{
    [StringLength(Organization.NameLength)]
    [Required]
    public string Name { get; init; }

    [StringLength(Organization.DescriptionFieldLength)]
    public string Description { get; init; }
}