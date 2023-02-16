using System;

namespace Domain.ValueObjects.Dates.Interfaces
{
    /// <summary>
    /// This interfaces is able to be used for your Domain models.
    /// </summary>
    public interface IHasDates
    {
        DateTimeOffset CreatedAt { get; }

        DateTimeOffset UpdatedAt { get; }

        void OnCreate(DateTimeOffset now);

        void OnUpdate(DateTimeOffset now);
    }
}