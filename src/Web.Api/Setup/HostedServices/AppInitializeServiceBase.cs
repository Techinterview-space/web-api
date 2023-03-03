using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TechInterviewer.Setup.HostedServices;

public abstract class AppInitializeServiceBase : IHostedService
{
    protected ILogger Logger { get; }

    // We need to inject the IServiceProvider so we can create any scoped service
    private readonly IServiceProvider _serviceProvider;

    protected AppInitializeServiceBase(ILogger logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected abstract Task InitAsync(IServiceScope scope, CancellationToken cancellationToken);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        await InitAsync(scope, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}