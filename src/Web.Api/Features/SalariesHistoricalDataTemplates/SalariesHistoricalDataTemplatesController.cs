using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.SalariesHistoricalDataTemplates.CreateTemplate;
using Web.Api.Features.SalariesHistoricalDataTemplates.DeleteAllTemplateRecords;
using Web.Api.Features.SalariesHistoricalDataTemplates.DeleteTemplate;
using Web.Api.Features.SalariesHistoricalDataTemplates.GetTemplates;
using Web.Api.Features.SalariesHistoricalDataTemplates.Shared;
using Web.Api.Features.SalariesHistoricalDataTemplates.UpdateTemplate;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.SalariesHistoricalDataTemplates;

[ApiController]
[Route("api/salaries-historical-data-templates")]
[HasAnyRole(Role.Admin)]
public class SalariesHistoricalDataTemplatesController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public SalariesHistoricalDataTemplatesController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("")]
    public async Task<Pageable<SalariesHistoricalDataRecordTemplateDto>> GetAll(
        [FromQuery] GetSalariesHistoricalDataRecordTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetSalariesHistoricalDataRecordTemplatesHandler, GetSalariesHistoricalDataRecordTemplatesQuery, Pageable<SalariesHistoricalDataRecordTemplateDto>>(
            request,
            cancellationToken);
    }

    [HttpPost("")]
    public async Task<SalariesHistoricalDataRecordTemplateDto> Create(
        [FromBody] CreateSalariesHistoricalDataRecordTemplateBodyRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<CreateSalariesHistoricalDataRecordTemplateHandler, CreateSalariesHistoricalDataRecordTemplateCommand, SalariesHistoricalDataRecordTemplateDto>(
            new CreateSalariesHistoricalDataRecordTemplateCommand(
                request),
            cancellationToken);
    }

    [HttpPost("{id:guid}")]
    public async Task<SalariesHistoricalDataRecordTemplateDto> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSalariesHistoricalDataRecordTemplateBodyRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<UpdateSalariesHistoricalDataRecordTemplateHandler, UpdateSalariesHistoricalDataRecordTemplateCommand, SalariesHistoricalDataRecordTemplateDto>(
            new UpdateSalariesHistoricalDataRecordTemplateCommand(
                id,
                request),
            cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeleteSalariesHistoricalDataRecordTemplateHandler, DeleteSalariesHistoricalDataRecordTemplateCommand, Nothing>(
            new DeleteSalariesHistoricalDataRecordTemplateCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}/records")]
    public async Task<IActionResult> DeleteAllTemplateRecords(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeleteAllTemplateRecordsHandler, DeleteAllTemplateRecordsCommand, Nothing>(
            new DeleteAllTemplateRecordsCommand(id),
            cancellationToken);

        return NoContent();
    }
}