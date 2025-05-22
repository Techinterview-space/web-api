using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Companies.CreateCompany;
using Web.Api.Features.Companies.GetCompany;
using Web.Api.Features.Companies.GetCompanyByAdmin;
using Web.Api.Features.Companies.SearchCompanies;
using Web.Api.Features.Companies.SearchCompaniesForAdmin;
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

    [HttpGet("for-admin")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> SearchCompaniesForAdmin(
        [FromQuery] SearchCompaniesForAdminQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _mediator.Send(
                new SearchCompaniesForAdminQuery(queryParams),
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
}