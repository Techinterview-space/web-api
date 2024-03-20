using System;
using MediatR;

namespace TechInterviewer.Features.Salaries.ExcludeFromStats;

public record ExcludeFromStatsCommand : IRequest<Unit>
{
    public ExcludeFromStatsCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}