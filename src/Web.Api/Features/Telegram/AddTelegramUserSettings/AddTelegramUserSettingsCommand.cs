using MediatR;
using Web.Api.Features.Telegram.GetTelegramUserSettings;

namespace Web.Api.Features.Telegram.AddTelegramUserSettings;

public record AddTelegramUserSettingsCommand
    : AddTelegramUserSettingsRequest
{
    public AddTelegramUserSettingsCommand(
        AddTelegramUserSettingsRequest request)
    {
        UserId = request.UserId;
        Username = request.Username;
        ChatId = request.ChatId;
        SendBotRegularStatsUpdates = request.SendBotRegularStatsUpdates;
    }
}