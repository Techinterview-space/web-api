using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Services.Correlation;
using Microsoft.AspNetCore.Http;

namespace Web.Api.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _nextInvoke;

    public CorrelationIdMiddleware(
        RequestDelegate nextInvoke)
    {
        _nextInvoke = nextInvoke;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        var correlationId = GetCorrelationId(context);
        context.Items.TryAdd(CorrelationIdAccessor.CorrelationIdHeaderName, correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append(CorrelationIdAccessor.CorrelationIdHeaderName, correlationId);
            return Task.CompletedTask;
        });

        await _nextInvoke(context);
    }

    private static string GetCorrelationId(
        HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdAccessor.CorrelationIdHeaderName, out var correlationId))
        {
            return correlationId!;
        }

        return Guid.NewGuid().ToString();
    }
}