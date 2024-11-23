using Domain.Entities.Telegram;
using MediatR;
using Telegram.Bot.Types;

namespace Web.Api.Features.Telegram.GetTelegramBotUsages;

public class GetOrCreateTelegramBotUsageCommand : IRequest<Unit>
{
    public GetOrCreateTelegramBotUsageCommand(
        string userName,
        string channelName,
        string receivedMessageTextOrNull,
        TelegramBotUsageType usageType)
    {
        UserName = userName;
        ChannelName = channelName;
        ReceivedMessageTextOrNull = receivedMessageTextOrNull;
        UsageType = usageType;
    }

    public string UserName { get; }

    public string ChannelName { get; }

    public string ReceivedMessageTextOrNull { get; }

    public TelegramBotUsageType UsageType { get; }
}
