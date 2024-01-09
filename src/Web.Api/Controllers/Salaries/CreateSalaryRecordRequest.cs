using Domain.Entities.Enums;
using Domain.Entities.Salaries;

namespace TechInterviewer.Controllers.Salaries;

public record CreateSalaryRecordRequest
{
    public double Value { get; init; }

    public int Quarter { get; init; }

    public int Year { get; init; }

    public Currency Currency { get; init; }

    public CompanyType Company { get; init; }

    public DeveloperGrade? Grage { get; init; }
}