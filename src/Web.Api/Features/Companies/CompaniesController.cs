using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Companies.AddCompanyReview;
using Web.Api.Features.Companies.ApproveReview;
using Web.Api.Features.Companies.CreateCompany;
using Web.Api.Features.Companies.DeleteCompanyReview;
using Web.Api.Features.Companies.GetCompany;
using Web.Api.Features.Companies.GetCompanyByAdmin;
using Web.Api.Features.Companies.MarkReviewOutdated;
using Web.Api.Features.Companies.SearchCompanies;
using Web.Api.Features.Companies.SearchReviewsToBeApproved;
using Web.Api.Features.Companies.SoftDeleteCompany;
using Web.Api.Features.Companies.UpdateCompany;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Companies;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> SearchCompanies(
        [FromQuery] SearchCompaniesQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _mediator.Send(
                new SearchCompaniesQuery(queryParams),
                cancellationToken));
    }

    [HttpPost("")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> CreateCompany(
        [FromBody] CreateCompanyBodyRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _mediator.Send(
                new CreateCompanyCommand(request),
                cancellationToken));
    }

    [HttpPost("{companyId:guid}")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> UpdateCompany(
        [FromRoute] Guid companyId,
        [FromBody] EditCompanyBodyRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _mediator.Send(
                new UpdateCompanyCommand(
                    companyId,
                    request),
                cancellationToken));
    }

    [HttpGet("{companyIdentifier}")]
    public async Task<IActionResult> GetCompany(
        string companyIdentifier,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _mediator.Send(
                new GetCompanyQuery(companyIdentifier),
                cancellationToken));
    }

    [HttpGet("{companyIdentifier}/for-admin")]
    public async Task<IActionResult> GetCompanyByAdmin(
        string companyIdentifier,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _mediator.Send(
                new GetCompanyByAdminQuery(companyIdentifier),
                cancellationToken));
    }

    [HttpDelete("{companyId:guid}")]
    public async Task<IActionResult> DeleteCompany(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _mediator.Send(
                new SoftDeleteCompanyCommand(companyId),
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