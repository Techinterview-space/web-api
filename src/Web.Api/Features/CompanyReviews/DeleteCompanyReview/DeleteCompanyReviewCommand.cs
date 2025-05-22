using System;
using MediatR;

namespace Web.Api.Features.CompanyReviews.DeleteCompanyReview;

public record DeleteCompanyReviewCommand : IRequest<Unit>
{
    public DeleteCompanyReviewCommand(
        Guid companyId,
        Guid reviewId)
    {
        CompanyId = companyId;
        ReviewId = reviewId;
    }

    public Guid CompanyId { get; }

    public Guid ReviewId { get; }
}