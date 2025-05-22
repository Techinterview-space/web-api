using System;
using MediatR;

namespace Web.Api.Features.CompanyReviews.ApproveReview;

public record ApproveReviewCommand : IRequest<Unit>
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