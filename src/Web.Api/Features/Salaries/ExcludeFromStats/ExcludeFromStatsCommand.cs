using System;
using MediatR;

namespace Web.Api.Features.Salaries.ExcludeFromStats;

public record ExcludeFromStatsCommand : IRequest<Unit>
{
    public ExcludeFromStatsCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}