using System;

namespace Domain.Validation.Exceptions;

public class DbUpdateConcurrencyException : InvalidOperationException
{
    public DbUpdateConcurrencyException(string error, Exception innerException)
        : base(error, innerException)
    {
    }
}