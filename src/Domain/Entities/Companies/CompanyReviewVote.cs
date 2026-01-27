using System;
using Domain.Entities.Users;

namespace Domain.Entities.Companies;

public class CompanyReviewVote
{
    public Guid ReviewId { get; protected set; }

    public virtual CompanyReview CompanyReview { get; protected set; }

    public long UserId { get; protected set; }

    public virtual User User { get; protected set; }

    public CompanyReviewVoteType VoteType { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public CompanyReviewVote(
        CompanyReview review,
        long userId,
        CompanyReviewVoteType voteType)
        : this(
            review.Id,
            userId,
            voteType)
    {
    }

    public CompanyReviewVote(
        Guid reviewId,
        long userId,
        CompanyReviewVoteType voteType)
    {
        ReviewId = reviewId;
        UserId = userId;
        VoteType = voteType;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    protected CompanyReviewVote()
    {
    }
}