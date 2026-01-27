using System;

namespace Domain.Validation.Exceptions;

public class DatabaseException : InvalidOperationException
{
    public DatabaseException(Exception innerException)
        : base("Exception during database command execution", innerException)
    {
    }
}