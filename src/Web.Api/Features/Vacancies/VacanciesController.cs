using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Vacancies.CreateVacancy;
using Web.Api.Features.Vacancies.DeleteVacancy;
using Web.Api.Features.Vacancies.Dtos;
using Web.Api.Features.Vacancies.GetVacancy;
using Web.Api.Features.Vacancies.SearchMyVacancies;
using Web.Api.Features.Vacancies.SearchVacancies;
using Web.Api.Features.Vacancies.UpdateVacancy;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Vacancies;

[ApiController]
[Route("api/vacancies")]
public class VacanciesController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public VacanciesController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("")]
    public async Task<IActionResult> SearchVacancies(
        [FromQuery] SearchVacanciesQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<SearchVacanciesHandler, SearchVacanciesQueryParams, Pageable<VacancyListItemDto>>(
                queryParams,
                cancellationToken));
    }

    [HttpGet("my")]
    [HasAnyRole]
    public async Task<IActionResult> SearchMyVacancies(
        [FromQuery] SearchMyVacanciesQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<SearchMyVacanciesHandler, SearchMyVacanciesQueryParams, SearchMyVacanciesResponse>(
                queryParams,
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVacancy(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<GetVacancyHandler, Guid, VacancyDto>(
                id,
                cancellationToken));
    }

    [HttpPost("")]
    [HasAnyRole]
    public async Task<IActionResult> CreateVacancy(
        [FromBody] CreateVacancyBodyRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<CreateVacancyHandler, CreateVacancyBodyRequest, VacancyDto>(
                request,
                cancellationToken));
    }

    [HttpPatch("{id:guid}")]
    [HasAnyRole]
    public async Task<IActionResult> UpdateVacancy(
        [FromRoute] Guid id,
        [FromBody] UpdateVacancyBodyRequest request,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<UpdateVacancyHandler, UpdateVacancyCommand, Nothing>(
            new UpdateVacancyCommand(id, request),
            cancellationToken);

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [HasAnyRole]
    public async Task<IActionResult> DeleteVacancy(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeleteVacancyHandler, DeleteVacancyCommand, Nothing>(
            new DeleteVacancyCommand(id),
            cancellationToken);

        return Ok();
    }
}
