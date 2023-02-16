using System;
using System.Threading.Tasks;
using MG.Utils.AspNetCore.Middlewares.Error;
using Microsoft.AspNetCore.Http;

namespace MG.Utils.AspNetCore.Middlewares
{
    public abstract class DebugExceptionHandlerMiddlewareBase : ExceptionHandlerMiddlewareBase
    {
        protected DebugExceptionHandlerMiddlewareBase(RequestDelegate next)
            : base(next)
        {
        }

        protected override Task WriteResponseAsync(HttpContext context, int statusCode, string message, Exception exception)
        {
            return new JsonErrorResponse(
                context: context,
                serializedError: Serialize(new DebugErrorDetails(statusCode, message, exception)),
                statusCode: statusCode).WriteAsync();
        }

        private class DebugErrorDetails : ErrorDetails
        {
            public DebugErrorDetails(int status, string message, Exception exception)
                : base(status, message)
            {
                Exception = exception;
            }

            public Exception Exception { get; }
        }
    }
}