using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Database;
using MG.Utils.AspNetCore.HostedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TechInterviewer.Setup;

public class AppInitializeService : AppInitializeServiceBase
{
    public AppInitializeService(ILogger<AppInitializeService> logger, IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    protected override async Task InitAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await MigrateAsync(database, cancellationToken);
    }

    private async Task MigrateAsync(DatabaseContext context, CancellationToken cancellationToken)
    {
        try
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Cannot migrate database", exception);
        }
    }
}