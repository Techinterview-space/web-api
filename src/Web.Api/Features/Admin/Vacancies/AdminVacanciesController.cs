using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Admin.Vacancies.ChangeVacancyStatus;
using Web.Api.Features.Admin.Vacancies.RestoreVacancy;
using Web.Api.Features.Admin.Vacancies.SearchVacanciesForAdmin;
using Web.Api.Features.Vacancies.Dtos;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Admin.Vacancies;

[ApiController]
[Route("api/admin/vacancies")]
[HasAnyRole(Role.Admin)]
public class AdminVacanciesController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public AdminVacanciesController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("")]
    public async Task<IActionResult> SearchVacancies(
        [FromQuery] SearchVacanciesForAdminQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<SearchVacanciesForAdminHandler, SearchVacanciesForAdminQueryParams, Pageable<VacancyListItemDto>>(
                queryParams,
                cancellationToken));
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        [FromRoute] Guid id,
        [FromBody] ChangeVacancyStatusBodyRequest request,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<ChangeVacancyStatusHandler, ChangeVacancyStatusCommand, Nothing>(
            new ChangeVacancyStatusCommand(id, request.Status),
            cancellationToken);

        return Ok();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> RestoreVacancy(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<RestoreVacancyHandler, RestoreVacancyCommand, Nothing>(
            new RestoreVacancyCommand(id),
            cancellationToken);

        return Ok();
    }
}
