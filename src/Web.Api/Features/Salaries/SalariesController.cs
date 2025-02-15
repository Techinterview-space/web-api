using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Salaries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Salaries.AddSalary;
using Web.Api.Features.Salaries.ExportCsv;
using Web.Api.Features.Salaries.GetAdminChart;
using Web.Api.Features.Salaries.GetSalaries;
using Web.Api.Features.Salaries.GetSalariesChart;
using Web.Api.Features.Salaries.GetSalariesChart.Charts;
using Web.Api.Features.Salaries.GetSelectBoxItems;
using Web.Api.Features.Salaries.Models;
using Web.Api.Features.Salaries.UpdateSalary;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Salaries;

[ApiController]
[Route("api/salaries")]
public class SalariesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalariesController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("select-box-items")]
    public async Task<SelectBoxItemsResponse> GetSelectBoxItems(
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new GetSelectBoxItemsQuery(),
            cancellationToken);
    }

    [HttpGet("salaries-adding-trend-chart")]
    public async Task<GetAddingTrendChartResponse> GetAddingTrendChart(
        [FromQuery] GetAddingTrendChartQuery request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new GetAddingTrendChartQuery
            {
                Grade = request.Grade,
                ProfessionsToInclude = new DeveloperProfessionsCollection(request.ProfessionsToInclude)
                    .ToList(),
                Cities = request.Cities,
                Skills = request.Skills,
                SalarySourceType = request.SalarySourceType,
                QuarterTo = request.QuarterTo,
                YearTo = request.YearTo,
            },
            cancellationToken);
    }

    [HttpGet("chart")]
    public Task<SalariesChartResponse> GetChart(
        [FromQuery] GetSalariesChartQuery request,
        CancellationToken cancellationToken)
    {
        return _mediator.Send(
            new GetSalariesChartQuery
            {
                Grade = request.Grade,
                ProfessionsToInclude = new DeveloperProfessionsCollection(request.ProfessionsToInclude).ToList(),
                Cities = request.Cities,
                Skills = request.Skills,
                SalarySourceTypes = request.SalarySourceTypes,
                QuarterTo = request.QuarterTo,
                YearTo = request.YearTo,
            },
            cancellationToken);
    }

    [HttpGet("")]
    public Task<Pageable<UserSalaryDto>> AllForPublic(
        [FromQuery] GetSalariesPaginatedQuery request,
        CancellationToken cancellationToken)
    {
        return _mediator.Send(
            new GetSalariesPaginatedQuery
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Grade = request.Grade,
                ProfessionsToInclude = new DeveloperProfessionsCollection(request.ProfessionsToInclude).ToList(),
                Cities = request.Cities,
                Skills = request.Skills,
                SalarySourceTypes = request.SalarySourceTypes,
                QuarterTo = request.QuarterTo,
                YearTo = request.YearTo,
            },
            cancellationToken);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            new ExportCsvQuery(),
            cancellationToken);

        return File(response.GetAsByteArray(), response.FileContentType, response.Filename);
    }

    [HttpPost("")]
    [HasAnyRole]
    public async Task<CreateOrEditSalaryRecordResponse> Create(
        [FromBody] AddSalaryCommand request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            request,
            cancellationToken);
    }

    [HttpPost("{id:guid}")]
    [HasAnyRole]
    public async Task<CreateOrEditSalaryRecordResponse> Update(
        [FromRoute] Guid id,
        [FromBody] EditSalaryRequest request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new UpdateSalaryCommand(
                id,
                request),
            cancellationToken);
    }
}