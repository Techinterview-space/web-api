using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Enums;
using Domain.Validation;
using Domain.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.CompanyReviews.AddCompanyReview;
using Web.Api.Features.CompanyReviews.ApproveReview;
using Web.Api.Features.CompanyReviews.DeleteCompanyReview;
using Web.Api.Features.CompanyReviews.GetRecentReviews;
using Web.Api.Features.CompanyReviews.MarkReviewOutdated;
using Web.Api.Features.CompanyReviews.SearchReviewsToBeApproved;
using Web.Api.Features.CompanyReviews.VoteForReview;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.CompanyReviews;

[ApiController]
[Route("api/companies")]
public class CompanyReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompanyReviewsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("reviews/recent")]
    public async Task<IActionResult> SearchRecentReviews(
        [FromQuery] PageModel queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _mediator.Send(
                new GetRecentCompanyReviewsQuery(queryParams),
                cancellationToken));
    }

    [HttpPost("{companyId:guid}/reviews")]
    [HasAnyRole]
    public async Task<IActionResult> AddCompanyReview(
        [FromRoute] Guid companyId,
        [FromBody] AddCompanyReviewBodyRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        await _mediator.Send(
            new AddCompanyReviewCommand(companyId, request),
            cancellationToken);

        return Ok();
    }

    [HttpGet("reviews/to-approve")]
    public async Task<IActionResult> SearchReviewsToBeApproved(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _mediator.Send(
                new SearchReviewsToBeApprovedQuery(),
                cancellationToken));
    }

    [HttpPost("{companyId:guid}/reviews/{reviewId:guid}/approve")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> ApproveReview(
        [FromRoute] Guid companyId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ApproveReviewCommand(companyId, reviewId),
            cancellationToken);

        return Ok();
    }

    [HttpPost("{companyId:guid}/reviews/{reviewId:guid}/outdate")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> MarkReviewOutdated(
        [FromRoute] Guid companyId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new MarkReviewOutdatedCommand(companyId, reviewId),
            cancellationToken);

        return Ok();
    }

    [HttpPost("{companyId:guid}/reviews/{reviewId:guid}/like")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> LikeReview(
        [FromRoute] Guid companyId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new VoteForReviewCommand(
                companyId,
                reviewId,
                CompanyReviewVoteType.Like),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{companyId:guid}/reviews/{reviewId:guid}/dislike")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> DislikeReview(
        [FromRoute] Guid companyId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new VoteForReviewCommand(
                companyId,
                reviewId,
                CompanyReviewVoteType.Dislike),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{companyId:guid}/reviews/{reviewId:guid}")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> DeleteCompanyReview(
        [FromRoute] Guid companyId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteCompanyReviewCommand(companyId, reviewId),
            cancellationToken);

        return Ok();
    }
}