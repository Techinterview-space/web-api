using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Entities.Companies;

namespace Web.Api.Features.CompanyReviews.AddCompanyReview;

public record AddCompanyReviewBodyRequest
{
    [Range(1, 5)]
    public int CultureAndValues { get; init; }

    [Range(1, 5)]
    public int? CodeQuality { get; init; }

    [Range(1, 5)]
    public int WorkLifeBalance { get; init; }

    [Range(1, 5)]
    public int Management { get; init; }

    [Range(1, 5)]
    public int CompensationAndBenefits { get; init; }

    [Range(1, 5)]
    public int CareerOpportunities { get; init; }

    public string Pros { get; init; }

    public string Cons { get; init; }

    public bool IWorkHere { get; init; }

    [NotDefaultValue]
    public CompanyEmploymentType UserEmployment { get; init; }
}