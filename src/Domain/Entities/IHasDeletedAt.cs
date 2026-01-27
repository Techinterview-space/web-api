using System;

namespace Domain.Entities;

public interface IHasDeletedAt
{
    DateTimeOffset? DeletedAt { get; }
}