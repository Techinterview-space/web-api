using System;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Exceptions;

namespace TechInterviewer.Controllers.Salaries.CreateSalaryRecord;

public record CreateSalaryRecordRequest
{
    public double Value { get; init; }

    public int Quarter { get; init; }

    public int Year { get; init; }

    public Currency Currency { get; init; }

    public CompanyType Company { get; init; }

    public DeveloperGrade? Grade { get; init; }

    public UserProfession Profession { get; init; }

    public void IsValidOrFail()
    {
        if (Value <= 0)
        {
            throw new BadRequestException("Value must be greater than 0");
        }

        if (Quarter is < 1 or > 4)
        {
            throw new BadRequestException("Quarter must be between 1 and 4");
        }

        var thisYear = DateTimeOffset.Now.Year;
        if (Year < 2000 || Year > thisYear)
        {
            throw new BadRequestException("Year must be between 2000 and current year");
        }

        if (Currency == default)
        {
            throw new BadRequestException("Currency must be specified");
        }

        if (Company == default)
        {
            throw new BadRequestException("Company must be specified");
        }

        if (Grade == DeveloperGrade.Unknown)
        {
            throw new BadRequestException("Grade must be valid");
        }

        if (Profession == UserProfession.Undefined)
        {
            throw new BadRequestException("Profession must be valid");
        }
    }
}