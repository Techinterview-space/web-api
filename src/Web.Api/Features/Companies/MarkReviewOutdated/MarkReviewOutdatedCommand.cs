using System;
using MediatR;

namespace Web.Api.Features.Companies.MarkReviewOutdated;

public record MarkReviewOutdatedCommand : IRequest<Unit>
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