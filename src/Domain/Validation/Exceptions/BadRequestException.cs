using System;

namespace Domain.Validation.Exceptions;

/// <summary>
/// Bad Request exception. Means that the client has sent the invalid request.
/// </summary>
public class BadRequestException : InvalidOperationException
{
    public BadRequestException()
        : base("Bad Request")
    {
    }

    public BadRequestException(string message)
        : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}