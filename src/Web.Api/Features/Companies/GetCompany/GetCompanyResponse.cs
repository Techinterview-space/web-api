using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.GetCompany;

public record GetCompanyResponse
{
    public GetCompanyResponse(
        CompanyDto company,
        bool userIsAllowedToLeaveReview,
        bool userHasAnyReview)
    {
        Company = company;
        UserIsAllowedToLeaveReview = userIsAllowedToLeaveReview;
        UserHasAnyReview = userHasAnyReview;
    }

    public CompanyDto Company { get; }

    public bool UserIsAllowedToLeaveReview { get; }

    public bool UserHasAnyReview { get; }
}