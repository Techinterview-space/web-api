using System;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;

namespace Domain.Services.Salaries;

public record UserSalaryDto
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
        Profession = salary.ProfessionEnum;
        City = salary.City;
        SkillId = salary.SkillId;
        WorkIndustryId = salary.WorkIndustryId;
        CreatedAt = salary.CreatedAt;
        UpdatedAt = salary.UpdatedAt;
    }

    public double Value { get; init; }

    public int Quarter { get; init; }

    public int Year { get; init; }

    public Currency Currency { get; init; }

    public CompanyType Company { get; init; }

    public DeveloperGrade? Grade { get; init; }

    public UserProfessionEnum Profession { get; init; }

    public KazakhstanCity? City { get; init; }

    public long? SkillId { get; init; }

    public long? WorkIndustryId { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}