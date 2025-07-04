using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Mediator;
using Infrastructure.Services.Telegram.Salaries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Web.Api.Features.Telegram.ProcessSalariesRelatedMessage;

namespace Web.Api.Features.Telegram;

public class SalariesTelegramBotHostedService
    : TelegramBotHostedServiceBase<SalariesTelegramBotHostedService, ISalariesTelegramBotClientProvider>
{
    public SalariesTelegramBotHostedService(
        ISalariesTelegramBotClientProvider botClientProvider,
        ILogger<SalariesTelegramBotHostedService> logger,
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
            UpdateType.ChosenInlineResult,
            UpdateType.ChannelPost,
        ];
    }

    protected override Task HandleUpdateAsync(
        IServiceScope scope,
        ITelegramBotClient client,
        Update updateRequest,
        CancellationToken cancellationToken)
    {
        if (updateRequest.Message is null &&
            updateRequest.InlineQuery is null &&
            updateRequest.ChosenInlineResult is null &&
            updateRequest.ChannelPost is null)
        {
            return Task.CompletedTask;
        }

        if (updateRequest.ChosenInlineResult is not null)
        {
            // Ignore commands in ChosenInlineResult
            Logger.LogInformation(
                "TELEGRAM_BOT. Salaries. Ignoring ChosenInlineResult with InlineMessageId: {InlineMessageId} " +
                "from {Name}. " +
                "Id {Id}. " +
                "IsBot {IsBot}",
                updateRequest.ChosenInlineResult.InlineMessageId,
                updateRequest.ChosenInlineResult.From.Username,
                updateRequest.ChosenInlineResult.From.Id,
                updateRequest.ChosenInlineResult.From.IsBot);

            return Task.CompletedTask;
        }

        var handler = scope.ServiceProvider.GetRequiredService<ProcessSalariesRelatedTelegramMessageHandler>();
        return handler.Handle(
            new ProcessTelegramMessageCommand(client, updateRequest),
            cancellationToken);
    }
}