using System;

namespace Domain.Exceptions;

public class DatabaseException : InvalidOperationException
{
    public DatabaseException(Exception innerException)
        : base("Exception during database command execution", innerException)
    {
    }
}