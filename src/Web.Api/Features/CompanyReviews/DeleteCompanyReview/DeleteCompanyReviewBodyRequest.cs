namespace Web.Api.Features.CompanyReviews.DeleteCompanyReview;

public record DeleteCompanyReviewBodyRequest
{
    public string Comment { get; init; }
}
