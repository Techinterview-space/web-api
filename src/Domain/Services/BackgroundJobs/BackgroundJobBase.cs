using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace Domain.Services.BackgroundJobs;

public abstract class BackgroundJobBase : IInvocable
{
    private readonly ILogger _logger;

    private string JobName => GetType().Name;

    protected BackgroundJobBase(ILogger logger)
    {
        _logger = logger;
    }

    protected abstract Task InvokeAsync();

#pragma warning disable UseAsyncSuffix // Use Async suffix
    public async Task Invoke()
#pragma warning restore UseAsyncSuffix // Use Async suffix
    {
        Stopwatch stopwatch = null;
        try
        {
            stopwatch = Stopwatch.StartNew();

            await InvokeAsync();

            stopwatch.Stop();

            _logger.LogInformation(
                "Finished background task {JobName}. Execution time: {ElapsedMilliseconds}", JobName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            stopwatch?.Stop();

            _logger.LogError(
                exception,
                "A task {JobName} has error: {Message}",
                JobName,
                exception.Message);
        }
    }
}