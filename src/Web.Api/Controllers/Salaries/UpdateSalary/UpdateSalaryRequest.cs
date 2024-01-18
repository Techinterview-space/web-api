using System;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Exceptions;

namespace TechInterviewer.Controllers.Salaries.UpdateSalary;

public record UpdateSalaryRequest
{
    public CompanyType Company { get; init; }

    public DeveloperGrade? Grade { get; init; }

    public void IsValidOrFail()
    {
        if (Company == default)
        {
            throw new BadRequestException("Company must be specified");
        }

        if (Grade == DeveloperGrade.Unknown)
        {
            throw new BadRequestException("Grade must be valid");
        }
    }
}