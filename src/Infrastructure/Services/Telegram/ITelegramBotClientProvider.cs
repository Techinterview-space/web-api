using Telegram.Bot;

namespace Infrastructure.Services.Telegram;

public interface ITelegramBotClientProvider
{
    ITelegramBotClient CreateClient();
}