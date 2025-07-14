using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.CompanyReviews.GetRecentReviews;

public record GetRecentCompanyReviewsRssQuery(
    int Page = 1,
    int PageSize = 50);