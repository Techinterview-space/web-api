using System;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;

namespace Domain.Entities.StatData.Salary;

public record SalaryBaseData
{
    public SalaryBaseData()
    {
    }

    public SalaryBaseData(
        UserSalary entity)
    {
        ProfessionId = entity.ProfessionId;
        Grade = entity.Grade.GetValueOrDefault(DeveloperGrade.Unknown);
        Value = entity.Value;
        CreatedAt = entity.CreatedAt;
    }

    public long? ProfessionId { get; init; }

    public DeveloperGrade Grade { get; init; }

    public double Value { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}