using System;

namespace Web.Api.Middlewares.Error;

public record ErrorDetails
{
    private const string DefaultServerErrorMessage = "Internal Server Error";

    public ErrorDetails(
        int status,
        string message = null,
        Exception exception = null)
    {
        Status = status;
        Message = message ?? exception?.Message ?? DefaultServerErrorMessage;
        ExceptionType = exception?.GetType().FullName;
        StackTrace = exception?.StackTrace;

        if (exception?.InnerException != null)
        {
            InnerExceptionMessage = exception.InnerException.Message;
            InnerExceptionType = exception.InnerException.GetType().FullName;
            InnerExceptionStackTrace = exception.InnerException.StackTrace;
        }
    }

    public int Status { get; }

    public string Message { get; }

    public string ExceptionType { get; }

    public string StackTrace { get; }

    public string InnerExceptionMessage { get; }

    public string InnerExceptionType { get; }

    public string InnerExceptionStackTrace { get; }

    public string RequestId => Guid.NewGuid().ToString();
}