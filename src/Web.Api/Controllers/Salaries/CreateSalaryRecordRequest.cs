using System;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Exceptions;

namespace TechInterviewer.Controllers.Salaries;

public record CreateSalaryRecordRequest
{
    public double Value { get; init; }

    public int Quarter { get; init; }

    [Range(2000, 3000)]
    public int Year { get; init; }

    [NotDefaultValue]
    public Currency Currency { get; init; }

    [NotDefaultValue]
    public CompanyType Company { get; init; }

    public DeveloperGrade? Grage { get; init; }

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
    }
}