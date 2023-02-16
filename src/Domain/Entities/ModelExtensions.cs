using System;

namespace Domain.Entities;

public static class ModelExtensions
{
    public static bool Active(this IHasDeletedAt deleted)
    {
        return deleted.DeletedAt == null;
    }

    public static T ActiveOrFail<T>(this T deleted)
        where T : IHasDeletedAt, IHasId
    {
        if (deleted.Active())
        {
            return deleted;
        }

        throw new InvalidOperationException(
            $"The entity of {typeof(T).Name} Id:{deleted.Id} has been deleted already");
    }
}