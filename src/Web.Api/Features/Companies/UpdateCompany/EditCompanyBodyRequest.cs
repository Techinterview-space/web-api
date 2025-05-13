using System.ComponentModel.DataAnnotations;
using Web.Api.Features.Companies.CreateCompany;

namespace Web.Api.Features.Companies.UpdateCompany;

public record EditCompanyBodyRequest : CreateCompanyBodyRequest
{
    [Required]
    public string Slug { get; init; }
}