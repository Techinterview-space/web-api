using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Telegram.GithubProfile;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Web.Api.Features.Telegram.GithubProfiles;

namespace Web.Api.Features.Telegram;

public class GithubProfileBotHostedService
    : TelegramBotHostedServiceBase<GithubProfileBotHostedService, IGithubProfileBotProvider>
{
    public GithubProfileBotHostedService(
        IGithubProfileBotProvider botClientProvider,
        ILogger<GithubProfileBotHostedService> logger,
        IServiceScopeFactory serviceScopeFactory)
        : base(botClientProvider, logger, serviceScopeFactory)
    {
    }

    protected override UpdateType[] GetUpdateTypes()
    {
        return
        [
            UpdateType.InlineQuery,
            UpdateType.Message,
            UpdateType.ChosenInlineResult
        ];
    }

    protected override Task HandleUpdateAsync(
        IServiceScope scope,
        ITelegramBotClient client,
        Update updateRequest,
        CancellationToken cancellationToken)
    {
        if (updateRequest.Message is null && updateRequest.InlineQuery is null && updateRequest.ChosenInlineResult is null)
        {
            return Task.CompletedTask;
        }

        if (updateRequest.ChosenInlineResult is not null)
        {
            Logger.LogInformation(
                "TELEGRAM_BOT. GithubProfile. Processing ChosenInlineResult with InlineMessageId: {InlineMessageId} " +
                "from {Name}. " +
                "Id {Id}. " +
                "IsBot {IsBot}",
                updateRequest.ChosenInlineResult.InlineMessageId,
                updateRequest.ChosenInlineResult.From.Username,
                updateRequest.ChosenInlineResult.From.Id,
                updateRequest.ChosenInlineResult.From.IsBot);

            return HandleChosenInlineResultAsync(scope, updateRequest, cancellationToken);
        }

        var handler = scope.ServiceProvider.GetRequiredService<ProcessGithubProfileTelegramMessageHandler>();
        return handler.Handle(
            new ProcessTelegramMessageCommand(client, updateRequest),
            cancellationToken);
    }

    private async Task HandleChosenInlineResultAsync(
        IServiceScope scope,
        Update updateRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<Infrastructure.Database.DatabaseContext>();
            
            var inlineReply = new Domain.Entities.Telegram.TelegramInlineReply(
                null, // No specific chat context for inline query results
                Domain.Entities.Telegram.TelegramBotType.GithubProfile);

            await context.SaveAsync(inlineReply, cancellationToken);

            Logger.LogInformation(
                "TELEGRAM_BOT. GithubProfile. Successfully saved ChosenInlineResult to database for user {UserId}",
                updateRequest.ChosenInlineResult.From.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "TELEGRAM_BOT. GithubProfile. Failed to save ChosenInlineResult to database for user {UserId}: {Message}",
                updateRequest.ChosenInlineResult.From.Id,
                ex.Message);
        }
    }
}