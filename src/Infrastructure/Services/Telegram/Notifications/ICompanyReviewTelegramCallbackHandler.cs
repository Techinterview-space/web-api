using Telegram.Bot;
using Telegram.Bot.Types;

namespace Infrastructure.Services.Telegram.Notifications;

public interface ICompanyReviewTelegramCallbackHandler
{
    Task<bool> TryHandleCallbackAsync(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken = default);
}
