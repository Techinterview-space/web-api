using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Validation.Exception;

public class EntityInvalidException : InvalidOperationException
{
    public EntityInvalidException(string message)
        : base(message)
    {
    }

    public static EntityInvalidException FromInstance<T>(ICollection<string> errors)
    {
        errors.ThrowIfNull(nameof(errors));
        if (!errors.Any())
        {
            throw new InvalidOperationException("Collection of errors could not be empty");
        }

        var message = $"Instance of {typeof(T).Name} is invalid";
        if (errors.Any())
        {
            message += "\r\n" + errors.Aggregate((result, item) => result + item + "\r\n");
        }

        return new EntityInvalidException(message);
    }
}