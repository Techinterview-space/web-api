using System;

namespace Web.Api.Features.Salaries.ApproveSalary;

public record ApproveSalaryCommand
{
    public ApproveSalaryCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}