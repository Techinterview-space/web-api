using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechInterviewer.Setup.HostedServices;

namespace TechInterviewer.Setup;

public class AppInitializeService : AppInitializeServiceBase
{
    public AppInitializeService(
        ILogger<AppInitializeService> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    protected override async Task InitAsync(
        IServiceScope scope,
        CancellationToken cancellationToken)
    {
        var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await MigrateAsync(database, cancellationToken);

        var telegramService = scope.ServiceProvider.GetRequiredService<TelegramBotService>();
        telegramService.StartReceiving(cancellationToken);
    }

    private async Task MigrateAsync(
        DatabaseContext context,
        CancellationToken cancellationToken)
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