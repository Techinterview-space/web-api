using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Web.Api.Features.Telegram;

namespace Web.Api.Setup.HostedServices;

public class AppInitializeService : IHostedService
{
    private readonly ILogger<AppInitializeService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AppInitializeService(
        ILogger<AppInitializeService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        await InitAsync(scope, cancellationToken);
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task InitAsync(
        IServiceScope scope,
        CancellationToken cancellationToken)
    {
        var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await MigrateAsync(database, cancellationToken);

        var telegramService = scope.ServiceProvider.GetRequiredService<TelegramBotService>();
        telegramService.StartReceiving(cancellationToken);
    }

    private static async Task MigrateAsync(
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