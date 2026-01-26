using System;

namespace Domain.Validation.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }

    public UnauthorizedException()
        : this("Unauthorized")
    {
    }
}
