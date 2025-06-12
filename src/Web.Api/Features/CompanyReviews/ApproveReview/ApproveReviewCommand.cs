using System;

namespace Web.Api.Features.CompanyReviews.ApproveReview;

public record ApproveReviewCommand
{
    public ApproveReviewCommand(
        Guid companyId,
        Guid reviewId)
    {
        CompanyId = companyId;
        ReviewId = reviewId;
    }

    public Guid CompanyId { get; }

    public Guid ReviewId { get; }
}