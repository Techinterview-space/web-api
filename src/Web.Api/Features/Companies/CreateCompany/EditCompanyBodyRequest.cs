using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.Companies.CreateCompany;

public record EditCompanyBodyRequest
{
    [Required]
    public string Name { get; init; }

    [Required]
    public string Description { get; init; }

    public List<string> Links { get; init; } = new List<string>();

    public string LogoUrl { get; init; }
}