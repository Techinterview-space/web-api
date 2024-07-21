using MediatR;
using Web.Api.Features.Telegram.ProcessMessage;
using Web.Api.Features.Telegram.ProcessMessage.UserCommands;

namespace Web.Api.Features.Telegram.ReplyWithSalaries;

public class ReplyWithSalariesCommand : IRequest<TelegramBotReplyData>
{
    public ReplyWithSalariesCommand(TelegramBotUserCommandParameters requestParams, string applicationName)
    {
        Params = requestParams;
        ApplicationName = applicationName;
    }

    public TelegramBotUserCommandParameters Params { get; }

    public string ApplicationName { get; }
}
