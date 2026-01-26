using System;

namespace Web.Api.Features.CompanyReviews.MarkReviewOutdated;

public record MarkReviewOutdatedCommand
{
    public MarkReviewOutdatedCommand(
        Guid companyId,
        Guid reviewId)
    {
        CompanyId = companyId;
        ReviewId = reviewId;
    }

    public Guid CompanyId { get; }

    public Guid ReviewId { get; }
}