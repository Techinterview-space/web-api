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
        CompanyType company,
        DeveloperGrade? grade,
        DateTimeOffset createdAt)
    {
        Value = value;
        Company = company;
        Grade = grade;
        CreatedAt = createdAt;
    }

    public double Value { get; init; }

    public CompanyType Company { get; init; }

    public DeveloperGrade? Grade { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}