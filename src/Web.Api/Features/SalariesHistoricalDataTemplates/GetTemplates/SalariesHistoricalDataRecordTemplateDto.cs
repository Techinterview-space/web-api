using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Domain.Entities.HistoricalRecords;

namespace Web.Api.Features.SalariesHistoricalDataTemplates.GetTemplates;

public record SalariesHistoricalDataRecordTemplateDto
{
    public SalariesHistoricalDataRecordTemplateDto()
    {
    }

    public SalariesHistoricalDataRecordTemplateDto(
        SalariesHistoricalDataRecordTemplate entity)
    {
        Id = entity.Id;
        ProfessionIds = entity.ProfessionIds;
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdatedAt;
    }

    public Guid Id { get; init; }

    public List<long> ProfessionIds { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public static readonly Expression<Func<SalariesHistoricalDataRecordTemplate, SalariesHistoricalDataRecordTemplateDto>> Transform = x => new SalariesHistoricalDataRecordTemplateDto
    {
        Id = x.Id,
        ProfessionIds = x.ProfessionIds,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}