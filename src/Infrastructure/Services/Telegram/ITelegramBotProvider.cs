using Telegram.Bot;

namespace Infrastructure.Services.Telegram;

public interface ITelegramBotProvider
{
    Task<ITelegramBotClient> CreateClientAsync(CancellationToken cancellationToken = default);
}