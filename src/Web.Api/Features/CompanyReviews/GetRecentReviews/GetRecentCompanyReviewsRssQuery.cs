namespace Web.Api.Features.CompanyReviews.GetRecentReviews;

#pragma warning disable SA1313
public record GetRecentCompanyReviewsRssQuery(
    int Page = 1,
    int PageSize = 50);