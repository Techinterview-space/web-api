using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Salaries.Admin.GetApprovedSalaries;
using Web.Api.Features.Salaries.Admin.GetExcludedFromStatsSalaries;
using Web.Api.Features.Salaries.Admin.GetSourcedSalaries;
using Web.Api.Features.Salaries.ApproveSalary;
using Web.Api.Features.Salaries.DeleteSalary;
using Web.Api.Features.Salaries.ExcludeFromStats;
using Web.Api.Features.Salaries.Models;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Salaries;

[ApiController]
[Route("api/salaries")]
[HasAnyRole(Role.Admin)]
public class SalariesAdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalariesAdminController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("all")]
    public async Task<Pageable<UserSalaryAdminDto>> GetAllAdmin(
        [FromQuery] GetApprovedSalariesQuery request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            request,
            cancellationToken);
    }

    [HttpGet("not-in-stats")]
    public async Task<Pageable<UserSalaryAdminDto>> GetAllNotShownInStats(
        [FromQuery] GetExcludedFromStatsSalariesQuery request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            request,
            cancellationToken);
    }

    [HttpGet("sourced-salaries")]
    public async Task<Pageable<UserSalaryAdminDto>> GetAllNotShownInStats(
        [FromQuery] GetSourcedSalariesQuery request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            request,
            cancellationToken);
    }

    [HttpPost("{id:guid}/approve")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveSalaryCommand(id), cancellationToken);
        return Ok();
    }

    [HttpPost("{id:guid}/exclude-from-stats")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> ExcludeFromStats(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ExcludeFromStatsCommand(id), cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSalaryCommand(id), cancellationToken);
        return Ok();
    }
}