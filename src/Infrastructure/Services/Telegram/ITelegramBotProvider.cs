using Telegram.Bot;

namespace Infrastructure.Services.Telegram;

public interface ITelegramBotProvider
{
    ITelegramBotClient CreateClient();
}