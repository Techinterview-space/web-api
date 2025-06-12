using Domain.ValueObjects.Pagination;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.CompanyReviews.GetRecentReviews;

public record GetRecentCompanyReviewsQuery : PageModel>
{
    public GetRecentCompanyReviewsQuery(
        PageModel page)
        : base(page.Page, page.PageSize)
    {
    }
}