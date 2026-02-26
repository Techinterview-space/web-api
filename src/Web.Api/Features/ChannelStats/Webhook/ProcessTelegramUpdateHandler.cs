using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Web.Api.Features.ChannelStats.Webhook;

public class ProcessTelegramUpdateHandler
    : IRequestHandler<ProcessTelegramUpdateRequest, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly ILogger<ProcessTelegramUpdateHandler> _logger;

    public ProcessTelegramUpdateHandler(
        DatabaseContext context,
        ILogger<ProcessTelegramUpdateHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Nothing> Handle(
        ProcessTelegramUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var alreadyExists = await _context.TelegramRawUpdates
            .AnyAsync(x => x.UpdateId == request.UpdateId, cancellationToken);

        if (alreadyExists)
        {
            _logger.LogInformation(
                "Telegram update {UpdateId} already exists, skipping",
                request.UpdateId);
            return Nothing.Value;
        }

        var rawUpdate = new TelegramRawUpdate(request.UpdateId, request.PayloadJson);
        await _context.SaveAsync(rawUpdate, cancellationToken);

        try
        {
            var update = JsonSerializer.Deserialize<Update>(request.PayloadJson, JsonBotAPI.Options);
            await ProcessUpdateAsync(update, cancellationToken);
            rawUpdate.MarkProcessed();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process Telegram update {UpdateId}",
                request.UpdateId);
            rawUpdate.MarkFailed(ex.Message);
        }

        await _context.TrySaveChangesAsync(cancellationToken);
        return Nothing.Value;
    }

    private async Task ProcessUpdateAsync(
        Update update,
        CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.ChannelPost:
                await HandleChannelPostAsync(update.ChannelPost, cancellationToken);
                break;

            case UpdateType.EditedChannelPost:
                await HandleEditedChannelPostAsync(update.EditedChannelPost, cancellationToken);
                break;

            case UpdateType.Message:
                await HandlePossibleCommentAsync(update.Message, cancellationToken);
                break;

            case UpdateType.EditedMessage:
                _logger.LogDebug("Ignoring edited message update for channel stats");
                break;

            case UpdateType.MessageReactionCount:
                await HandleMessageReactionCountAsync(update.MessageReactionCount, cancellationToken);
                break;

            default:
                _logger.LogDebug(
                    "Ignoring Telegram update type {UpdateType}",
                    update.Type);
                break;
        }
    }

    private async Task HandleMessageReactionCountAsync(
        MessageReactionCountUpdated reactionCountUpdated,
        CancellationToken cancellationToken)
    {
        if (reactionCountUpdated?.Chat == null)
        {
            return;
        }

        var channel = await FindMonitoredChannelAsync(reactionCountUpdated.Chat.Id, cancellationToken);
        if (channel == null)
        {
            return;
        }

        var post = await _context.ChannelPosts
            .FirstOrDefaultAsync(
                x => x.MonitoredChannelId == channel.Id
                     && x.TelegramMessageId == reactionCountUpdated.MessageId,
                cancellationToken);

        if (post == null)
        {
            _logger.LogDebug(
                "Reaction count update received for unknown post {MessageId} in channel {ChannelName}",
                reactionCountUpdated.MessageId,
                channel.ChannelName);
            return;
        }

        var likeCount = reactionCountUpdated.Reactions?.Sum(x => x.TotalCount) ?? 0;
        post.UpdateLikeCount(likeCount);

        _logger.LogInformation(
            "Updated like count for post {MessageId} in channel {ChannelName}: {LikeCount}",
            post.TelegramMessageId,
            channel.ChannelName,
            likeCount);
    }

    private async Task HandleChannelPostAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        if (message?.Chat == null)
        {
            return;
        }

        var channel = await FindMonitoredChannelAsync(message.Chat.Id, cancellationToken);
        if (channel == null)
        {
            return;
        }

        var existingPost = await _context.ChannelPosts
            .FirstOrDefaultAsync(
                x => x.MonitoredChannelId == channel.Id && x.TelegramMessageId == message.MessageId,
                cancellationToken);

        if (existingPost != null)
        {
            return;
        }

        var postReference = BuildPostReference(message.Chat.Username, message.MessageId);
        var textPreview = TruncateText(message.Text ?? message.Caption, 500);

        var post = new ChannelPost(
            channel.Id,
            message.MessageId,
            message.Date,
            postReference,
            textPreview);

        await _context.SaveAsync(post, cancellationToken);

        _logger.LogInformation(
            "Saved channel post {MessageId} for channel {ChannelName}",
            message.MessageId,
            channel.ChannelName);
    }

    private async Task HandleEditedChannelPostAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        if (message?.Chat == null)
        {
            return;
        }

        var channel = await FindMonitoredChannelAsync(message.Chat.Id, cancellationToken);
        if (channel == null)
        {
            return;
        }

        var existingPost = await _context.ChannelPosts
            .FirstOrDefaultAsync(
                x => x.MonitoredChannelId == channel.Id && x.TelegramMessageId == message.MessageId,
                cancellationToken);

        if (existingPost == null)
        {
            return;
        }

        var textPreview = TruncateText(message.Text ?? message.Caption, 500);
        existingPost.UpdateTextPreview(textPreview);
    }

    private async Task HandlePossibleCommentAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        if (message?.Chat == null || !message.IsTopicMessage)
        {
            if (message?.ReplyToMessage?.ForwardOrigin != null)
            {
                await TryMapCommentViaForwardAsync(message, cancellationToken);
                return;
            }

            return;
        }

        await TryMapCommentViaDiscussionChatAsync(message, cancellationToken);
    }

    private async Task TryMapCommentViaDiscussionChatAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        var channel = await _context.MonitoredChannels
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.DiscussionChatExternalId == message.Chat.Id && x.IsActive,
                cancellationToken);

        if (channel == null)
        {
            return;
        }

        if (message.MessageThreadId == null)
        {
            return;
        }

        var post = await _context.ChannelPosts
            .FirstOrDefaultAsync(
                x => x.MonitoredChannelId == channel.Id
                     && x.TelegramMessageId == message.MessageThreadId.Value,
                cancellationToken);

        if (post != null)
        {
            post.IncrementCommentCount();
            _logger.LogInformation(
                "Incremented comment count for post {MessageId} in channel {ChannelName}",
                post.TelegramMessageId,
                channel.ChannelName);
        }
    }

    private async Task TryMapCommentViaForwardAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        if (message?.ReplyToMessage == null)
        {
            return;
        }

        var replyTo = message.ReplyToMessage;
        if (replyTo.ForwardOrigin is not MessageOriginChannel originChannel)
        {
            return;
        }

        var channel = await FindMonitoredChannelAsync(originChannel.Chat.Id, cancellationToken);
        if (channel == null)
        {
            return;
        }

        var post = await _context.ChannelPosts
            .FirstOrDefaultAsync(
                x => x.MonitoredChannelId == channel.Id
                     && x.TelegramMessageId == originChannel.MessageId,
                cancellationToken);

        if (post != null)
        {
            post.IncrementCommentCount();
            _logger.LogInformation(
                "Incremented comment count (via forward) for post {MessageId} in channel {ChannelName}",
                post.TelegramMessageId,
                channel.ChannelName);
        }
    }

    private async Task<MonitoredChannel> FindMonitoredChannelAsync(
        long chatId,
        CancellationToken cancellationToken)
    {
        return await _context.MonitoredChannels
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ChannelExternalId == chatId && x.IsActive,
                cancellationToken);
    }

    private static string BuildPostReference(string channelUsername, long messageId)
    {
        if (string.IsNullOrEmpty(channelUsername))
        {
            return null;
        }

        return $"https://t.me/{channelUsername}/{messageId}";
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        return text.Length <= maxLength ? text : text[..maxLength];
    }
}
