using System;
using Domain.Entities.Salaries;
using Domain.Validation.Exceptions;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.AddSalary;

public record AddSalaryCommand : EditSalaryRequest, MediatR.IRequest<CreateOrEditSalaryRecordResponse>
{
    public double Value { get; init; }

    public int Quarter { get; init; }

    public int Year { get; init; }

    public Currency Currency { get; init; }

    public override void IsValidOrFail()
    {
        base.IsValidOrFail();

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
    }
}