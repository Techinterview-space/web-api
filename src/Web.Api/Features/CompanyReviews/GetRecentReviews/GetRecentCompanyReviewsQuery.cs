using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.CompanyReviews.GetRecentReviews;

public record GetRecentCompanyReviewsQuery : PageModel
{
    public GetRecentCompanyReviewsQuery(
        PageModel page)
        : base(page.Page, page.PageSize)
    {
    }
}