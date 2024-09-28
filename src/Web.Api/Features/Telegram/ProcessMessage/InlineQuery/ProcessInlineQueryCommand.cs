using System.Collections.Generic;
using Domain.Entities.Salaries;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Web.Api.Features.Telegram.ProcessMessage.InlineQuery;

public class ProcessInlineQueryCommand : IRequest<Unit>
{
    public ProcessInlineQueryCommand(
        ITelegramBotClient client,
        Update updateRequest,
        List<Profession> allProfessions)
    {
        BotClient = client;
        UpdateRequest = updateRequest;
        AllProfessions = allProfessions;
    }

    public ITelegramBotClient BotClient { get; }

    public Update UpdateRequest { get; }

    public List<Profession> AllProfessions { get; }
}


