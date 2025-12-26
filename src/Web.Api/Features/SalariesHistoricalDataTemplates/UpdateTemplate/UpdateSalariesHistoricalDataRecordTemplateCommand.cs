using System;
using Web.Api.Features.SalariesHistoricalDataTemplates.Shared;

namespace Web.Api.Features.SalariesHistoricalDataTemplates.UpdateTemplate;

public record UpdateSalariesHistoricalDataRecordTemplateCommand
    : UpdateSalariesHistoricalDataRecordTemplateBodyRequest
{
    public Guid TemplateId { get; }

    public UpdateSalariesHistoricalDataRecordTemplateCommand(
        Guid templateId,
        UpdateSalariesHistoricalDataRecordTemplateBodyRequest request)
    {
        TemplateId = templateId;
        Name = request.Name;
        ProfessionIds = request.ProfessionIds;
    }
}