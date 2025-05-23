﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Web.Api.Middlewares;

public class LoggingMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    private readonly RequestDelegate _next;

    private readonly IReadOnlyCollection<Type> _exceptionsToIgnore = new List<Type>
    {
        typeof(AuthenticationException),
        typeof(BadRequestException),
        typeof(EntityInvalidException),
        typeof(NotFoundException),
    };

    public LoggingMiddleware(
        ILoggerFactory loggerFactory,
        RequestDelegate next)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<LoggingMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (!Ignore(exception))
            {
                _logger.LogError(
                    exception,
                    "Unhandled error occured. Message {Message}",
                    exception.Message);
            }

            throw;
        }
    }

    private bool Ignore(Exception exception)
    {
        return _exceptionsToIgnore.Contains(exception.GetType());
    }
}