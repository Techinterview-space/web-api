using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.CompanyReviews.GetRecentReviews;

namespace Web.Api.Features.CompanyReviews;

[ApiController]
[Route("api/company-reviews")]
public class CompanyReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompanyReviewsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("recent")]
    public async Task<IActionResult> SearchCompanies(
        [FromQuery] PageModel queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _mediator.Send(
                new GetRecentCompanyReviewsQuery(queryParams),
                cancellationToken));
    }
}