namespace Web.Api.Features.CompanyReviews.VoteForReview;

public record VoteForReviewResponse
{
    public VoteForReviewResponse(
        bool result)
    {
        Result = result;
    }

    public bool Result { get; }
}