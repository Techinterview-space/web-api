using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;

namespace Infrastructure.Salaries;

public record UserSalaryDto : UserSalarySimpleDto
{
    public UserSalaryDto()
    {
    }

    public UserSalaryDto(
        UserSalary salary)
    {
        Value = salary.Value;
        Quarter = salary.Quarter;
        Year = salary.Year;
        Currency = salary.Currency;
        Company = salary.Company;
        Grade = salary.Grade;
        City = salary.City;
        Age = salary.Age;
        YearOfStartingWork = salary.YearOfStartingWork;
        Gender = salary.Gender;
        SkillId = salary.SkillId;
        WorkIndustryId = salary.WorkIndustryId;
        ProfessionId = salary.ProfessionId;
        CreatedAt = salary.CreatedAt;
        UpdatedAt = salary.UpdatedAt;
    }

    public int Quarter { get; init; }

    public int Year { get; init; }

    public Currency Currency { get; init; }

    public KazakhstanCity? City { get; init; }

    public int? Age { get; init; }

    public int? YearOfStartingWork { get; init; }

    public int? YearsOfExperience => YearOfStartingWork.HasValue
        ? DateTimeOffset.UtcNow.Year - YearOfStartingWork
        : null;

    public Gender? Gender { get; init; }

    public long? SkillId { get; init; }

    public long? WorkIndustryId { get; init; }

    public long? ProfessionId { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    private bool RequireAdditionalData =>
        SkillId.HasValue ||
        WorkIndustryId.HasValue ||
        ProfessionId.HasValue ||
        YearOfStartingWork.HasValue ||
        Gender.HasValue ||
        Grade.HasValue;
}