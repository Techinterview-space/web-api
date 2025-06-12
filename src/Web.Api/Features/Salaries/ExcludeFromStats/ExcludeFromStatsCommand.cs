using System;

namespace Web.Api.Features.Salaries.ExcludeFromStats;

public record ExcludeFromStatsCommand
{
    public ExcludeFromStatsCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}