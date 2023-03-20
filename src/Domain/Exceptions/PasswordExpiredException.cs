using System;

namespace Domain.Exceptions;

public class PasswordExpiredException : InvalidOperationException
{
    // password must be at least 8 characters in length and contain at least 1 upper case, numeric, and special character
    public PasswordExpiredException(string message)
        : base(message)
    {
    }
}