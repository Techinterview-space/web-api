using System;

namespace Domain.Exceptions;

public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string message)
        : base(message)
    {
    }

    public static ResourceNotFoundException CreateFromEntity<TEntity>(long id)
    {
        return new ($"Did not find any {typeof(TEntity).Name} by id={id}");
    }

    public static ResourceNotFoundException CreateFromEntity<TEntity>(Guid id)
    {
        return new ($"Did not find any {typeof(TEntity).Name} by id={id}");
    }

    public static ResourceNotFoundException CreateFromEntity<TEntity>(string selector)
    {
        return new ($"Did not find any {typeof(TEntity).Name} by {selector}");
    }
}