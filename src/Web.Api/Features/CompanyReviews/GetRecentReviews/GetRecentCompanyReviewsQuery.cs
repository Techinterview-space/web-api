﻿using Domain.ValueObjects.Pagination;
using MediatR;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.CompanyReviews.GetRecentReviews;

public record GetRecentCompanyReviewsQuery : PageModel, IRequest<Pageable<CompanyReviewDto>>
{
    public GetRecentCompanyReviewsQuery(
        PageModel page)
        : base(page.Page, page.PageSize)
    {
    }
}