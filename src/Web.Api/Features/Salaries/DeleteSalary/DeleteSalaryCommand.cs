using System;

namespace Web.Api.Features.Salaries.DeleteSalary;

public record DeleteSalaryCommand
{
    public DeleteSalaryCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}