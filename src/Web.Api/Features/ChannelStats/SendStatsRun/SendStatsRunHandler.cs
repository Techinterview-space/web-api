using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Telegram.ChannelStats;
using Infrastructure.Services.Telegram.ReplyMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Web.Api.Features.ChannelStats.SendStatsRun;

public class SendStatsRunHandler
    : Infrastructure.Services.Mediator.IRequestHandler<SendStatsRunRequest, SendStatsRunResponse>
{
    private readonly DatabaseContext _context;
    private readonly IChannelStatsBotProvider _botProvider;
    private readonly ILogger<SendStatsRunHandler> _logger;

    public SendStatsRunHandler(
        DatabaseContext context,
        IChannelStatsBotProvider botProvider,
        ILogger<SendStatsRunHandler> logger)
    {
        _context = context;
        _botProvider = botProvider;
        _logger = logger;
    }

    public async Task<SendStatsRunResponse> Handle(
        SendStatsRunRequest request,
        CancellationToken cancellationToken)
    {
        var run = await _context.MonthlyStatsRuns
            .Include(x => x.MonitoredChannel)
            .FirstOrDefaultAsync(x => x.Id == request.RunId, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<Domain.Entities.ChannelStats.MonthlyStatsRun>(request.RunId);

        var client = await _botProvider.CreateClientAsync(cancellationToken);
        if (client is null)
        {
            return new SendStatsRunResponse
            {
                Success = false,
                ErrorMessage = "Telegram bot is disabled",
            };
        }

        try
        {
            var message = ChannelStatsReplyMessageBuilder.Build(
                run, run.MonitoredChannel.ChannelName);

            await client.SendMessage(
                run.MonitoredChannel.ChannelExternalId,
                message.ReplyText,
                parseMode: message.ParseMode,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Sent stats run {RunId} to channel {ChannelName} (ExternalId: {ExternalId})",
                run.Id,
                run.MonitoredChannel.ChannelName,
                run.MonitoredChannel.ChannelExternalId);

            return new SendStatsRunResponse { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send stats run {RunId} to channel {ChannelName}",
                run.Id,
                run.MonitoredChannel.ChannelName);

            return new SendStatsRunResponse
            {
                Success = false,
                ErrorMessage = "Failed to send message to Telegram",
            };
        }
    }
}
