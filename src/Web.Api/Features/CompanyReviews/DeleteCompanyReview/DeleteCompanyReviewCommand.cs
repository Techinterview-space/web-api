using System;

namespace Web.Api.Features.CompanyReviews.DeleteCompanyReview;

public record DeleteCompanyReviewCommand
{
    public DeleteCompanyReviewCommand(
        Guid companyId,
        Guid reviewId,
        string comment = null)
    {
        CompanyId = companyId;
        ReviewId = reviewId;
        Comment = comment;
    }

    public Guid CompanyId { get; }

    public Guid ReviewId { get; }

    public string Comment { get; }
}
