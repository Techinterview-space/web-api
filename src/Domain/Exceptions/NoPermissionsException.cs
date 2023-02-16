using System;

namespace Domain.Exceptions;

public class NoPermissionsException : Exception
{
    public NoPermissionsException(string message)
        : base(message)
    {
    }

    public NoPermissionsException()
        : this("The user has no permissions to do this operation")
    {
    }
}