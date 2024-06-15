using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.BackgroundJobs;

public abstract class InvocableJobBase<TJob> : IInvocable
{
    protected InvocableJobBase(
        ILogger<TJob> logger)
    {
        Logger = logger;
    }

    protected ILogger<TJob> Logger { get; }

    public async Task Invoke()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        try
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            await ExecuteAsync(cancellationTokenSource.Token);

            var elapsed = GetElapsedMs(currentTimestamp);
            Logger.LogInformation(
                "Job {JobName} executed successfully in {ElapsedMilliseconds} ms",
                GetType().Name,
                elapsed);
        }
        catch (Exception e)
        {
            var elapsed = GetElapsedMs(currentTimestamp);
            Logger.LogError(
                e,
                "An error occurred while executing the job: {JobName}. Error: {ErrorMessage}. Elapsed time: {ElapsedMilliseconds} ms",
                GetType().Name,
                e.Message,
                elapsed);
        }
    }

    public abstract Task ExecuteAsync(
        CancellationToken cancellationToken = default);

    private long GetElapsedMs(
        long start)
    {
        return (Stopwatch.GetTimestamp() - start) / (Stopwatch.Frequency / 1000);
    }
}