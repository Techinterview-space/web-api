using System;
using Domain.ValueObjects.Dates.Interfaces;

namespace Domain.Entities;

public abstract class HasDatesBase : IHasDates
{
    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    public virtual void OnCreate(DateTimeOffset now)
    {
        CreatedAt = UpdatedAt = now;
    }

    public virtual void OnUpdate(DateTimeOffset now)
    {
        UpdatedAt = now;
    }
}