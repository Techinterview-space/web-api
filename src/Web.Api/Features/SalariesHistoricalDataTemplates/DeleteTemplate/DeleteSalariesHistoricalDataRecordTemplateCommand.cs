using System;

namespace Web.Api.Features.SalariesHistoricalDataTemplates.DeleteTemplate;

public record DeleteSalariesHistoricalDataRecordTemplateCommand
{
    public DeleteSalariesHistoricalDataRecordTemplateCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}