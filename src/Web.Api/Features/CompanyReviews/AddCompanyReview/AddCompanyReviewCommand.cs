using System;

namespace Web.Api.Features.CompanyReviews.AddCompanyReview;

public record AddCompanyReviewCommand : AddCompanyReviewBodyRequest
{
    public AddCompanyReviewCommand(
        Guid companyId,
        AddCompanyReviewBodyRequest request)
    {
        CompanyId = companyId;
        CultureAndValues = request.CultureAndValues;
        CodeQuality = request.CodeQuality;
        WorkLifeBalance = request.WorkLifeBalance;
        Management = request.Management;
        CompensationAndBenefits = request.CompensationAndBenefits;
        CareerOpportunities = request.CareerOpportunities;
        Pros = request.Pros?.Trim();
        Cons = request.Cons?.Trim();
        IWorkHere = request.IWorkHere;
        UserEmployment = request.UserEmployment;
    }

    public Guid CompanyId { get; }
}