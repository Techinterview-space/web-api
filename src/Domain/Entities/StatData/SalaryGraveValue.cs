using Domain.Entities.Enums;

namespace Domain.Entities.StatData;

public record SalaryGraveValue
{
    public DeveloperGrade Grade { get; init; }

    public double Value { get; init; }
}