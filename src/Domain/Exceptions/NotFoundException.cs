using System;

namespace Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public static NotFoundException CreateFromEntity<TEntity>(long id)
    {
        return new ($"Did not find any {typeof(TEntity).Name} by id={id}");
    }

    public static NotFoundException CreateFromEntity<TEntity>(Guid id)
    {
        return new ($"Did not find any {typeof(TEntity).Name} by id={id}");
    }

    public static NotFoundException CreateFromEntity<TEntity>(string selector)
    {
        return new ($"Did not find any {typeof(TEntity).Name} by {selector}");
    }
}