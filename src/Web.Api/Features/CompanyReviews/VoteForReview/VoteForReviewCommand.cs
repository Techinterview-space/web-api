using System;
using Domain.Attributes;
using Domain.Entities.Companies;

namespace Web.Api.Features.CompanyReviews.VoteForReview;

public record VoteForReviewCommand
{
    public VoteForReviewCommand(
        Guid companyId,
        Guid reviewId,
        CompanyReviewVoteType voteType)
    {
        CompanyId = companyId;
        ReviewId = reviewId;
        VoteType = voteType;
    }

    [NotDefaultValue]
    public Guid CompanyId { get; }

    [NotDefaultValue]
    public Guid ReviewId { get; }

    [NotDefaultValue]
    public CompanyReviewVoteType VoteType { get; }
}