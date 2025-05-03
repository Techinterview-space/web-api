using System;
using Domain.Entities.Enums;

namespace Domain.Entities.StatData;

public record SalaryBaseData
{
    public long? ProfessionId { get; init; }

    public DeveloperGrade Grade { get; init; }

    public double Value { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}