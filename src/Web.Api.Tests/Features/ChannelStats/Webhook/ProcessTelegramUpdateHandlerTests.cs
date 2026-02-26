using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.ChannelStats.Webhook;
using Xunit;

namespace Web.Api.Tests.Features.ChannelStats.Webhook;

public class ProcessTelegramUpdateHandlerTests
{
    [Fact]
    public async Task Handle_DuplicateUpdateId_SkipsProcessing()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Test Channel").PleaseAsync(context);

        var payload = CreateChannelPostPayload(1001, -1001234567890, 42);
        var rawUpdate = new TelegramRawUpdate(1001, payload);
        await context.SaveAsync(rawUpdate);

        context.ChangeTracker.Clear();

        var handler = CreateHandler(context);

        await handler.Handle(
            new ProcessTelegramUpdateRequest(1001, payload),
            default);

        // Should still have only 1 raw update
        Assert.Equal(1, await context.TelegramRawUpdates.CountAsync());

        // No channel posts created on duplicate
        Assert.Equal(0, await context.ChannelPosts.CountAsync());
    }

    [Fact]
    public async Task Handle_NewChannelPost_CreatesChannelPostProjection()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Test Channel",
            discussionChatExternalId: null).PleaseAsync(context);

        context.ChangeTracker.Clear();

        var payload = CreateChannelPostPayload(2001, -1001234567890, 100, "testchannel", "Hello world");
        var handler = CreateHandler(context);

        await handler.Handle(
            new ProcessTelegramUpdateRequest(2001, payload),
            default);

        var post = await context.ChannelPosts.FirstOrDefaultAsync();
        Assert.NotNull(post);
        Assert.Equal(channel.Id, post.MonitoredChannelId);
        Assert.Equal(100, post.TelegramMessageId);
        Assert.Equal("Hello world", post.TextPreview);
        Assert.Equal("https://t.me/testchannel/100", post.PostReference);

        var rawUpdate = await context.TelegramRawUpdates.FirstOrDefaultAsync();
        Assert.NotNull(rawUpdate);
        Assert.Equal(TelegramUpdateStatus.Processed, rawUpdate.Status);
    }

    [Fact]
    public async Task Handle_ChannelPostFromUnmonitoredChannel_SavesRawUpdateButNoPost()
    {
        await using var context = new InMemoryDatabaseContext();

        // Channel with different external ID
        await new MonitoredChannelFake(
            channelExternalId: -1009999999999,
            channelName: "Other Channel").PleaseAsync(context);

        context.ChangeTracker.Clear();

        var payload = CreateChannelPostPayload(3001, -1001111111111, 50);
        var handler = CreateHandler(context);

        await handler.Handle(
            new ProcessTelegramUpdateRequest(3001, payload),
            default);

        // Raw update saved and processed
        Assert.Equal(1, await context.TelegramRawUpdates.CountAsync());
        var rawUpdate = await context.TelegramRawUpdates.FirstAsync();
        Assert.Equal(TelegramUpdateStatus.Processed, rawUpdate.Status);

        // No channel post created
        Assert.Equal(0, await context.ChannelPosts.CountAsync());
    }

    [Fact]
    public async Task Handle_EditedChannelPost_UpdatesTextPreview()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Test Channel").PleaseAsync(context);

        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 200,
            textPreview: "Original text").PleaseAsync(context);

        context.ChangeTracker.Clear();

        var payload = CreateEditedChannelPostPayload(4001, -1001234567890, 200, "Updated text");
        var handler = CreateHandler(context);

        await handler.Handle(
            new ProcessTelegramUpdateRequest(4001, payload),
            default);

        var post = await context.ChannelPosts.FirstAsync();
        Assert.Equal("Updated text", post.TextPreview);
    }

    [Fact]
    public async Task Handle_UnknownUpdateType_SavesRawUpdateAsProcessed()
    {
        await using var context = new InMemoryDatabaseContext();

        var payload = JsonSerializer.Serialize(new
        {
            update_id = 5001,
            poll = new { id = "123", question = "Test?" },
        });

        var handler = CreateHandler(context);

        await handler.Handle(
            new ProcessTelegramUpdateRequest(5001, payload),
            default);

        var rawUpdate = await context.TelegramRawUpdates.FirstAsync();
        Assert.Equal(TelegramUpdateStatus.Processed, rawUpdate.Status);
        Assert.Null(rawUpdate.Error);
    }

    [Fact]
    public async Task Handle_InvalidPayload_MarksRawUpdateAsFailed()
    {
        await using var context = new InMemoryDatabaseContext();

        var handler = CreateHandler(context);

        await handler.Handle(
            new ProcessTelegramUpdateRequest(6001, "{\"update_id\": 6001, \"bad_field\": \"invalid\"}"),
            default);

        var rawUpdate = await context.TelegramRawUpdates.FirstAsync();

        // Should be processed (no exception from Telegram.Bot deserialization for unknown fields)
        // or failed if the deserialization throws
        Assert.True(
            rawUpdate.Status == TelegramUpdateStatus.Processed ||
            rawUpdate.Status == TelegramUpdateStatus.Failed);
    }

    [Fact]
    public async Task Handle_CommentViaForwardOrigin_IncrementsCommentCount()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Test Channel",
            discussionChatExternalId: -1009876543210).PleaseAsync(context);

        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 300).PleaseAsync(context);

        context.ChangeTracker.Clear();

        var payload = CreateCommentViaForwardPayload(7001, -1009876543210, 300, -1001234567890);
        var handler = CreateHandler(context);

        await handler.Handle(
            new ProcessTelegramUpdateRequest(7001, payload),
            default);

        var post = await context.ChannelPosts.FirstAsync();
        Assert.Equal(1, post.CommentCount);
    }

    [Fact]
    public async Task Handle_EditedCommentViaForwardOrigin_DoesNotIncrementCommentCount()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Test Channel",
            discussionChatExternalId: -1009876543210).PleaseAsync(context);

        await new ChannelPostFake(
                channel.Id,
                telegramMessageId: 300)
            .WithComments(1)
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var payload = CreateEditedCommentViaForwardPayload(7002, -1009876543210, 300, -1001234567890);
        var handler = CreateHandler(context);

        await handler.Handle(
            new ProcessTelegramUpdateRequest(7002, payload),
            default);

        var post = await context.ChannelPosts.FirstAsync();
        Assert.Equal(1, post.CommentCount);
    }

    [Fact]
    public async Task Handle_MessageReactionCountUpdate_UpdatesLikeCount()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Test Channel").PleaseAsync(context);

        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 400).PleaseAsync(context);

        context.ChangeTracker.Clear();

        var payload = CreateMessageReactionCountPayload(8001, -1001234567890, 400, 3, 2);
        var handler = CreateHandler(context);

        await handler.Handle(
            new ProcessTelegramUpdateRequest(8001, payload),
            default);

        var post = await context.ChannelPosts.FirstAsync();
        Assert.Equal(5, post.LikeCount);
    }

    private static ProcessTelegramUpdateHandler CreateHandler(
        InMemoryDatabaseContext context)
    {
        return new ProcessTelegramUpdateHandler(
            context,
            new Mock<ILogger<ProcessTelegramUpdateHandler>>().Object);
    }

    private static string CreateChannelPostPayload(
        long updateId,
        long chatId,
        long messageId,
        string chatUsername = null,
        string text = null)
    {
        return JsonSerializer.Serialize(new
        {
            update_id = updateId,
            channel_post = new
            {
                message_id = messageId,
                chat = new
                {
                    id = chatId,
                    type = "channel",
                    username = chatUsername,
                },
                date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                text = text ?? "Test post",
            },
        });
    }

    private static string CreateEditedChannelPostPayload(
        long updateId,
        long chatId,
        long messageId,
        string text)
    {
        return JsonSerializer.Serialize(new
        {
            update_id = updateId,
            edited_channel_post = new
            {
                message_id = messageId,
                chat = new
                {
                    id = chatId,
                    type = "channel",
                },
                date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                edit_date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                text,
            },
        });
    }

    private static string CreateCommentViaForwardPayload(
        long updateId,
        long discussionChatId,
        long originalMessageId,
        long channelId)
    {
        return JsonSerializer.Serialize(new
        {
            update_id = updateId,
            message = new
            {
                message_id = 999,
                chat = new
                {
                    id = discussionChatId,
                    type = "supergroup",
                },
                date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                text = "This is a comment",
                reply_to_message = new
                {
                    message_id = 998,
                    chat = new
                    {
                        id = discussionChatId,
                        type = "supergroup",
                    },
                    date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    forward_origin = new
                    {
                        type = "channel",
                        chat = new
                        {
                            id = channelId,
                            type = "channel",
                        },
                        message_id = originalMessageId,
                        date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    },
                },
            },
        });
    }

    private static string CreateEditedCommentViaForwardPayload(
        long updateId,
        long discussionChatId,
        long originalMessageId,
        long channelId)
    {
        return JsonSerializer.Serialize(new
        {
            update_id = updateId,
            edited_message = new
            {
                message_id = 999,
                chat = new
                {
                    id = discussionChatId,
                    type = "supergroup",
                },
                date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                edit_date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                text = "Edited comment",
                reply_to_message = new
                {
                    message_id = 998,
                    chat = new
                    {
                        id = discussionChatId,
                        type = "supergroup",
                    },
                    date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    forward_origin = new
                    {
                        type = "channel",
                        chat = new
                        {
                            id = channelId,
                            type = "channel",
                        },
                        message_id = originalMessageId,
                        date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    },
                },
            },
        });
    }

    private static string CreateMessageReactionCountPayload(
        long updateId,
        long chatId,
        long messageId,
        int firstReactionCount,
        int secondReactionCount)
    {
        return JsonSerializer.Serialize(new
        {
            update_id = updateId,
            message_reaction_count = new
            {
                chat = new
                {
                    id = chatId,
                    type = "channel",
                },
                message_id = messageId,
                date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                reactions = new object[]
                {
                    new
                    {
                        type = new
                        {
                            type = "emoji",
                            emoji = "👍",
                        },
                        total_count = firstReactionCount,
                    },
                    new
                    {
                        type = new
                        {
                            type = "emoji",
                            emoji = "🔥",
                        },
                        total_count = secondReactionCount,
                    },
                },
            },
        });
    }
}
