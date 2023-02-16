using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MG.Utils.AspNetCore.Middlewares.Error;
using Microsoft.AspNetCore.Http;

namespace MG.Utils.AspNetCore.Middlewares
{
    public abstract class ExceptionHandlerMiddlewareBase
    {
        private readonly RequestDelegate _next;

        protected ExceptionHandlerMiddlewareBase(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(context, e);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = StatusCodes.Status500InternalServerError;
            string message = null;

            if (exception.GetType() == typeof(NotImplementedException))
            {
                message = "The endpoint is not implemented yet. Please, keep calm and wait for a while";
            }
            else if (StatusCodeConversion.TryGetValue(exception.GetType(), out int status))
            {
                statusCode = status;
                message = FormatErrorMessage(exception.Message);
            }

            await WriteResponseAsync(context, statusCode, message, exception);
        }

        protected virtual Task WriteResponseAsync(
            HttpContext context, int statusCode, string message, Exception exception)
        {
            return new JsonErrorResponse(
                context: context,
                serializedError: Serialize(new ErrorDetails(statusCode, message)),
                statusCode: statusCode).WriteAsync();
        }

        protected abstract Dictionary<Type, int> StatusCodeConversion { get; }

        protected abstract string Serialize<T>([NotNull] T instance);

        protected virtual string FormatErrorMessage([NotNull] string message)
        {
            return message;
        }
    }
}