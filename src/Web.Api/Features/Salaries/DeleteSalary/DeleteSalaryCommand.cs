using System;
using MediatR;

namespace TechInterviewer.Features.Salaries.DeleteSalary;

public record DeleteSalaryCommand : IRequest<Unit>
{
    public DeleteSalaryCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}