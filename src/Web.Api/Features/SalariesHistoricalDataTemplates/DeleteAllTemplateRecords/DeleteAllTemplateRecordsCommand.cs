using System;

namespace Web.Api.Features.SalariesHistoricalDataTemplates.DeleteAllTemplateRecords;

public record DeleteAllTemplateRecordsCommand
{
    public DeleteAllTemplateRecordsCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}