using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Infrastructure.Database;
using Infrastructure.Services.Telegram;
using Infrastructure.Services.Telegram.ReplyMessages;
using Infrastructure.Services.Telegram.Salaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Web.Api.Features.BackgroundJobs;

public class TelegramSalariesRegularStatsUpdateJob
    : InvocableJobBase<TelegramSalariesRegularStatsUpdateJob>
{
    private readonly DatabaseContext _context;
    private readonly ISalariesTelegramBotClientProvider _botClientProvider;

    public TelegramSalariesRegularStatsUpdateJob(
        ILogger<TelegramSalariesRegularStatsUpdateJob> logger,
        DatabaseContext context,
        ISalariesTelegramBotClientProvider botClientProvider)
        : base(logger)
    {
        _context = context;
        _botClientProvider = botClientProvider;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await _context.TelegramUserSettings
            .Where(x => x.SendBotRegularStatsUpdates)
            .ToListAsync(cancellationToken);

        if (settings.Count == 0)
        {
            Logger.LogInformation("No users to send regular stats updates to.");
            return;
        }

        var client = _botClientProvider.CreateClient();
        if (client is null)
        {
            Logger.LogWarning("Telegram bot is disabled.");
            return;
        }

        var messageToSend = await new StatsReplyMessageBuilder(_context)
            .BuildAsync(cancellationToken);

        var failedToSend = new List<(TelegramUserSettings Settings, Exception Ex)>();

        foreach (var settingsEntity in settings)
        {
            try
            {
                await client.SendMessage(
                    settingsEntity.ChatId,
                    messageToSend.ReplyText,
                    parseMode: messageToSend.ParseMode,
                    cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                failedToSend.Add((settingsEntity, e));
            }
        }

        if (failedToSend.Count > 0)
        {
            Logger.LogError(
                "Failed to send regular stats updates to {Count} users. Errors: {Errors}",
                failedToSend.Count,
                failedToSend.Select(x => x.Ex.Message + ", " + x.Ex.GetType().FullName));
        }
    }
}