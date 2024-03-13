using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace Domain.Telegram;

public class TelegramBotService
{
    private static readonly Dictionary<DeveloperGrade, string> _options = new ()
    {
        { DeveloperGrade.Junior, DeveloperGrade.Junior.ToString() },
        { DeveloperGrade.Middle, "Middle" },
        { DeveloperGrade.Senior, "Senior" },
        { DeveloperGrade.Lead, "Lead" },
    };

    private readonly IConfiguration _configuration;

    public TelegramBotService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ProcessMessageAsync(
        Update updateRequest,
        CancellationToken cancellationToken)
    {
        if (updateRequest.Message is null)
        {
            return;
        }

        var client = CreateClient();
        var chatId = updateRequest.Message.Chat.Id;

        switch (updateRequest.Type)
        {
            case UpdateType.InlineQuery:
                var results = new List<InlineQueryResult>();
                var counter = 0;
                foreach (var option in _options)
                {
                    results.Add(new InlineQueryResultArticle(
                            $"{counter}", // we use the counter as an id for inline query results
                            option.Key.ToString(), // inline query result title
                            new InputTextMessageContent(option.Value)));
                    counter++;
                }

                var inlineQueryId = updateRequest.InlineQuery!.Id;
                await client.AnswerInlineQueryAsync(inlineQueryId, results, cancellationToken: cancellationToken);
                break;

            default:
                await client.SendTextMessageAsync(chatId, "Hello " + updateRequest.Message.From?.Username ?? "stranger", cancellationToken: cancellationToken);
                break;
        }
    }

    private TelegramBotClient CreateClient()
    {
        var token = _configuration["Telegram:BotToken"];
        if (string.IsNullOrEmpty(token))
        {
            throw new BadRequestException("Token is not set");
        }

        return new TelegramBotClient(token);
    }
}