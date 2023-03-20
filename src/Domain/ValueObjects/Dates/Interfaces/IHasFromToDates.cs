using System;

namespace Domain.ValueObjects.Dates.Interfaces
{
    public interface IHasFromToDates
    {
        DateTimeOffset From { get; set; }

        DateTimeOffset? To { get; set; }
    }
}