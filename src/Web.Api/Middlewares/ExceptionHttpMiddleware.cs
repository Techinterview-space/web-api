﻿using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Services.Correlation;
using Microsoft.AspNetCore.Http;
using Web.Api.Middlewares.Error;

namespace Web.Api.Middlewares;

public class ExceptionHttpMiddleware
{
    internal static readonly Dictionary<Type, int> StatusCodeTypes = new ()
    {
        { typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized },
        { typeof(AuthenticationException), StatusCodes.Status401Unauthorized },
        { typeof(NoPermissionsException), StatusCodes.Status403Forbidden },
        { typeof(NotFoundException), StatusCodes.Status404NotFound },
        { typeof(BadRequestException), StatusCodes.Status400BadRequest },
        { typeof(EntityInvalidException), StatusCodes.Status400BadRequest },
        { typeof(InputValidationException), StatusCodes.Status422UnprocessableEntity },
        { typeof(InvalidOperationException), StatusCodes.Status400BadRequest },
        { typeof(DbUpdateConcurrencyException), StatusCodes.Status409Conflict }
    };

    private readonly RequestDelegate _next;

    public ExceptionHttpMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        string message = null;

        if (exception.GetType() == typeof(NotImplementedException))
        {
            message = "The endpoint is not implemented yet. Please, keep calm and wait for a while";
        }
        else if (StatusCodeTypes.TryGetValue(exception.GetType(), out int status))
        {
            statusCode = status;
            message = exception.Message;
        }

        await WriteResponseAsync(
            context,
            statusCode,
            message,
            exception);
    }

    private Task WriteResponseAsync(
        HttpContext context,
        int statusCode,
        string message,
        Exception exception)
    {
        var correlationId = new CorrelationIdAccessor(context).GetValue();
        var serialized = Serialize(
            new ErrorDetails(
                statusCode,
                message,
                exception,
                correlationId));

        return new JsonErrorResponse(
            context: context,
            serializedError: serialized,
            statusCode: statusCode)
            .WriteAsync();
    }

    private string Serialize<T>(
        T instance)
    {
        return System.Text.Json.JsonSerializer.Serialize(instance);
    }
}