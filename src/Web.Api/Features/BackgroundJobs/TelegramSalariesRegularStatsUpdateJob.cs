﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Web.Api.Features.Telegram;

namespace Web.Api.Features.BackgroundJobs;

public class TelegramSalariesRegularStatsUpdateJob
    : InvocableJobBase<TelegramSalariesRegularStatsUpdateJob>
{
    private readonly DatabaseContext _context;
    private readonly TelegramBotClientProvider _botClientProvider;

    public TelegramSalariesRegularStatsUpdateJob(
        ILogger<TelegramSalariesRegularStatsUpdateJob> logger,
        DatabaseContext context,
        TelegramBotClientProvider botClientProvider)
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

        if (!settings.Any())
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

        var dayAgo = DateTime.UtcNow.AddDays(-1);
        var allSalariesCount = await _context.Salaries
            .CountAsync(cancellationToken);

        var addedRelevantSalariesCount = await _context.Salaries
            .Where(x => x.UseInStats)
            .Where(x => x.CreatedAt >= dayAgo)
            .CountAsync(cancellationToken);

        var addedIrrelevantSalariesCount = await _context.Salaries
            .Where(x => !x.UseInStats)
            .Where(x => x.CreatedAt >= dayAgo)
            .CountAsync(cancellationToken);

        var surveyPassedCount = await _context.SalariesSurveyReplies
            .Where(x => x.CreatedAt >= dayAgo)
            .CountAsync(cancellationToken);

        var messageToSend = $@"
Всего анкет: {allSalariesCount}

За последние сутки:
- Добавлено релевантных анкет: +{addedRelevantSalariesCount}
- Добавлено нерелевантных анкет: +{addedIrrelevantSalariesCount}
- Прошли опрос: +{surveyPassedCount}";

        var failedToSend = new List<(TelegramUserSettings Settings, Exception Ex)>();

        foreach (var settingsEntity in settings)
        {
            try
            {
                await client.SendTextMessageAsync(
                    settingsEntity.ChatId,
                    messageToSend,
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