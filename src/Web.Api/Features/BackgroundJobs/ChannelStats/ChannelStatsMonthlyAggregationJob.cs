using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Infrastructure.Services.ChannelStats;
using Infrastructure.Services.Telegram.ChannelStats;
using Infrastructure.Services.Telegram.ReplyMessages;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Web.Api.Features.BackgroundJobs.ChannelStats;

public class ChannelStatsMonthlyAggregationJob : InvocableJobBase<ChannelStatsMonthlyAggregationJob>
{
    private readonly IChannelStatsAggregationService _aggregationService;
    private readonly IChannelStatsBotProvider _botProvider;

    public ChannelStatsMonthlyAggregationJob(
        ILogger<ChannelStatsMonthlyAggregationJob> logger,
        IChannelStatsAggregationService aggregationService,
        IChannelStatsBotProvider botProvider)
        : base(logger)
    {
        _aggregationService = aggregationService;
        _botProvider = botProvider;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTimeOffset.UtcNow;

        if (!IsLastDayOfMonth(nowUtc))
        {
            Logger.LogInformation(
                "ChannelStatsMonthlyAggregationJob skipped: {Date} is not the last day of the month",
                nowUtc.Date.ToString("yyyy-MM-dd"));
            return;
        }

        Logger.LogInformation(
            "ChannelStatsMonthlyAggregationJob starting: {Date} is the last day of the month",
            nowUtc.Date.ToString("yyyy-MM-dd"));

        var result = await _aggregationService.RunAsync(
            StatsTriggerSource.Scheduled,
            nowUtc,
            cancellationToken);

        Logger.LogInformation(
            "ChannelStatsMonthlyAggregationJob completed: {RunCount} channels processed, {ErrorCount} errors",
            result.Runs.Count,
            result.Errors.Count);

        await SendStatsToChannelsAsync(result, cancellationToken);
    }

    internal async Task SendStatsToChannelsAsync(
        ChannelStatsAggregationResult result,
        CancellationToken cancellationToken)
    {
        if (result.Runs.Count == 0)
        {
            return;
        }

        var client = await _botProvider.CreateClientAsync(cancellationToken);
        if (client is null)
        {
            Logger.LogWarning("ChannelStats Telegram bot is disabled, skipping message sending");
            return;
        }

        var failedToSend = new List<(long ChannelId, string ChannelName, Exception Ex)>();

        foreach (var run in result.Runs)
        {
            if (!result.Channels.TryGetValue(run.MonitoredChannelId, out var channel))
            {
                Logger.LogWarning(
                    "Channel not found for MonitoredChannelId {ChannelId}, skipping message",
                    run.MonitoredChannelId);
                continue;
            }

            try
            {
                var message = ChannelStatsReplyMessageBuilder.Build(run, channel.ChannelName);

                await client.SendMessage(
                    channel.ChannelExternalId,
                    message.ReplyText,
                    parseMode: message.ParseMode,
                    cancellationToken: cancellationToken);

                Logger.LogInformation(
                    "Sent stats summary to channel {ChannelName} (ExternalId: {ExternalId})",
                    channel.ChannelName,
                    channel.ChannelExternalId);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    "Failed to send stats to channel {ChannelName} (ExternalId: {ExternalId})",
                    channel.ChannelName,
                    channel.ChannelExternalId);

                failedToSend.Add((channel.ChannelExternalId, channel.ChannelName, ex));
            }
        }

        if (failedToSend.Count > 0)
        {
            Logger.LogError(
                "Failed to send stats to {Count} channel(s)",
                failedToSend.Count);
        }
    }

    private static bool IsLastDayOfMonth(DateTimeOffset dateUtc)
    {
        return dateUtc.Day == DateTime.DaysInMonth(dateUtc.Year, dateUtc.Month);
    }
}
