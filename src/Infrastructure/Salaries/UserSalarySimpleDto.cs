using Domain.Entities.Enums;
using Domain.Entities.Salaries;

namespace Infrastructure.Salaries;

public record UserSalarySimpleDto
{
    public UserSalarySimpleDto()
    {
    }

    public UserSalarySimpleDto(
        double value,
        int quarter,
        int year,
        CompanyType company,
        DeveloperGrade? grade,
        DateTimeOffset createdAt)
    {
        Value = value;
        Quarter = quarter;
        Year = year;
        Company = company;
        Grade = grade;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public double Value { get; init; }

    public int Quarter { get; init; }

    public int Year { get; init; }

    public CompanyType Company { get; init; }

    public DeveloperGrade? Grade { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}