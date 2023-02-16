using System;

namespace MG.Utils.Abstract.Entities;

public interface IHasDeletedAt
{
    DateTimeOffset? DeletedAt { get; }
}