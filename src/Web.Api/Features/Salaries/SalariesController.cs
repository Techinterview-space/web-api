using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Salaries;
using Infrastructure.Services.Mediator;
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
    private readonly IServiceProvider _serviceProvider;

    public SalariesController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("select-box-items")]
    public async Task<SelectBoxItemsResponse> GetSelectBoxItems(
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetSelectBoxItemsHandler, GetSelectBoxItemsQuery, SelectBoxItemsResponse>(
            new GetSelectBoxItemsQuery(),
            cancellationToken);
    }

    [HttpGet("salaries-adding-trend-chart")]
    public async Task<GetAddingTrendChartResponse> GetAddingTrendChart(
        [FromQuery] GetAddingTrendChartQuery request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetAddingTrendChartHandler, GetAddingTrendChartQuery, GetAddingTrendChartResponse>(
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
        return _serviceProvider.HandleBy<GetSalariesChartHandler, GetSalariesChartQuery, SalariesChartResponse>(
            new GetSalariesChartQuery
            {
                Grade = request.Grade,
                SelectedProfessionIds = new DeveloperProfessionsCollection(request.SelectedProfessionIds).ToList(),
                Cities = request.Cities,
                Skills = request.Skills,
                SalarySourceTypes = request.SalarySourceTypes,
                QuarterTo = request.QuarterTo,
                YearTo = request.YearTo,
                DateTo = request.DateTo,
                AllowReadonly = request.AllowReadonly,
            },
            cancellationToken);
    }

    [HttpGet("")]
    public Task<Pageable<UserSalaryDto>> AllForPublic(
        [FromQuery] GetSalariesPaginatedQuery request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<GetSalariesPaginatedQueryHandler, GetSalariesPaginatedQuery, Pageable<UserSalaryDto>>(
            new GetSalariesPaginatedQuery
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Grade = request.Grade,
                SelectedProfessionIds = new DeveloperProfessionsCollection(request.SelectedProfessionIds).ToList(),
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
        var response = await _serviceProvider.HandleBy<ExportCsvHandler, Nothing, SalariesCsvResponse>(
            Nothing.Value,
            cancellationToken);

        return File(response.GetAsByteArray(), response.FileContentType, response.Filename);
    }

    [HttpPost("")]
    [HasAnyRole]
    public async Task<CreateOrEditSalaryRecordResponse> Create(
        [FromBody] AddSalaryCommand request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<AddSalaryHandler, AddSalaryCommand, CreateOrEditSalaryRecordResponse>(
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
        return await _serviceProvider.HandleBy<UpdateSalaryHandler, UpdateSalaryCommand, CreateOrEditSalaryRecordResponse>(
            new UpdateSalaryCommand(
                id,
                request),
            cancellationToken);
    }
}