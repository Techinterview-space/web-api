using System;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;

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
        Grage = salary.Grage;
        Profession = salary.Profession;
        CreatedAt = salary.CreatedAt;
    }

    public double Value { get; init; }

    public int Quarter { get; init; }

    public int Year { get; init; }

    public Currency Currency { get; init; }

    public CompanyType Company { get; init; }

    public DeveloperGrade? Grage { get; init; }

    public UserProfession Profession { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}