using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IServiceProvider _serviceProvider;

    public CompaniesController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("")]
    public async Task<IActionResult> SearchCompanies(
        [FromQuery] SearchCompaniesQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<SearchCompaniesHandler, SearchCompaniesQueryParams, SearchCompaniesResponse>(
                queryParams,
                cancellationToken));
    }

    [HttpGet("for-admin")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> SearchCompaniesForAdmin(
        [FromQuery] SearchCompaniesForAdminQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.GetRequiredService<SearchCompaniesForAdminHandler>()
                .Handle(queryParams, cancellationToken));
    }

    [HttpPost("")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> CreateCompany(
        [FromBody] CreateCompanyBodyRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.GetRequiredService<CreateCompanyHandler>()
                .Handle(request, cancellationToken));
    }

    [HttpPost("{companyId:guid}")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> UpdateCompany(
        [FromRoute] Guid companyId,
        [FromBody] EditCompanyBodyRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.GetRequiredService<UpdateCompanyHandler>()
                .Handle(
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
            await _serviceProvider.HandleBy<GetCompanyHandler, string, GetCompanyResponse>(
                companyIdentifier,
                cancellationToken));
    }

    [HttpGet("{companyIdentifier}/for-admin")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> GetCompanyByAdmin(
        string companyIdentifier,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.GetRequiredService<GetCompanyByAdminHandler>()
                .Handle(companyIdentifier, cancellationToken));
    }

    [HttpDelete("{companyId:guid}")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> DeleteCompany(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.GetRequiredService<SoftDeleteCompanyHandler>()
            .Handle(new SoftDeleteCompanyCommand(companyId), cancellationToken);

        return Ok();
    }
}