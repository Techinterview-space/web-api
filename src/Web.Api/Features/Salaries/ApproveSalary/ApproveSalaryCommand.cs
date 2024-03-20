using System;
using MediatR;

namespace TechInterviewer.Features.Salaries.ApproveSalary;

public record ApproveSalaryCommand : IRequest<Unit>
{
    public ApproveSalaryCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}