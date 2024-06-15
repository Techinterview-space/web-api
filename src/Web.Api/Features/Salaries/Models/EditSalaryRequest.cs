using System;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Validation.Exceptions;

namespace Web.Api.Features.Salaries.Models;

public record EditSalaryRequest
{
    public DeveloperGrade Grade { get; init; }

    public KazakhstanCity? City { get; init; }

    public int? Age { get; init; }

    public int? YearOfStartingWork { get; init; }

    public Gender? Gender { get; init; }

    public long? SkillId { get; init; }

    public long? WorkIndustryId { get; init; }

    public long? ProfessionId { get; init; }

    public CompanyType Company { get; init; }

    public virtual void IsValidOrFail()
    {
        if (Grade == DeveloperGrade.Unknown)
        {
            throw new BadRequestException("Grade must be valid");
        }

        if (City == KazakhstanCity.Undefined)
        {
            throw new BadRequestException("City must be valid");
        }

        if (Company == default)
        {
            throw new BadRequestException("Company must be specified");
        }

        if (ProfessionId is null or <= 0)
        {
            throw new BadRequestException("Profession must be specified");
        }

        if (Gender == Domain.Enums.Gender.Undefined)
        {
            throw new BadRequestException("Gender must be specified");
        }

        if (Age is < 10 or > 120)
        {
            throw new BadRequestException("Invalid age");
        }

        if (YearOfStartingWork.HasValue)
        {
            const int minYear = 1960;
            if (YearOfStartingWork.Value > DateTimeOffset.UtcNow.Year || YearOfStartingWork.Value < minYear)
            {
                throw new BadRequestException($"Year of starting work must be between current year and {minYear}");
            }
        }
    }
}