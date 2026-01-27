using Domain.Entities.Companies;
using Domain.Enums;
using Infrastructure.Database;
using Infrastructure.Services.Global;
using Infrastructure.Services.Telegram.Salaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.Services.Telegram.Notifications;

public class TelegramAdminNotificationService : ITelegramAdminNotificationService
{
    private readonly ISalariesTelegramBotClientProvider _botClientProvider;
    private readonly ILogger<TelegramAdminNotificationService> _logger;
    private readonly DatabaseContext _context;
    private readonly IGlobal _globalSettings;

    public TelegramAdminNotificationService(
        ISalariesTelegramBotClientProvider botClientProvider,
        ILogger<TelegramAdminNotificationService> logger,
        DatabaseContext context,
        IGlobal globalSettings)
    {
        _botClientProvider = botClientProvider;
        _logger = logger;
        _context = context;
        _globalSettings = globalSettings;
    }

    public async Task NotifyAboutNewCompanyReviewAsync(
        CompanyReview review,
        Company company,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await TrySendNotificationAsync(
                review,
                company,
                cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogCritical(
                e,
                "Error while sending notification about new review {ReviewId} for company {CompanyId}",
                review.Id,
                company.Id);
        }
    }

    public async Task TrySendNotificationAsync(
        CompanyReview review,
        Company company,
        CancellationToken cancellationToken = default)
    {
        var admins = await _context.TelegramUserSettings
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .Where(x => x.User.UserRoles.Any(r => r.RoleId == Role.Admin))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (admins.Count == 0)
        {
            _logger.LogInformation("No admins found. Skipping notification");
            return;
        }

        var botClient = _botClientProvider.CreateClient();
        if (botClient is null)
        {
            _logger.LogWarning("Telegram bot client is null. Skipping notification");
            return;
        }

        var pros = FormatReviewContent(review.Pros);
        var cons = FormatReviewContent(review.Cons);

        var message = $"<b>Новый отзыв о компании {EscapeHtml(company.Name)}</b>\n\n" +
                      $"<b>Рейтинг:</b> {review.TotalRating}\n\n" +
                      $"<b>Плюсы:</b>\n{pros}\n\n" +
                      $"<b>Минусы:</b>\n{cons}\n\n" +
                      $"<a href=\"{_globalSettings.FrontendBaseUrl}/admin/companies/reviews-to-approve\">Перейти к модерации</a>";

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Одобрить", $"review_approve:{review.Id}"),
                InlineKeyboardButton.WithCallbackData("❌ Отклонить", $"review_reject:{review.Id}")
            }
        });

        foreach (var admin in admins)
        {
            await botClient.SendMessage(
                admin.ChatId,
                message,
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Sent notification to admin {UserEmail} ({ChatId}) about new review {ReviewId}",
                admin.User.Email,
                admin.ChatId,
                review.Id);
        }
    }

    private static string FormatReviewContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "<i>Не указано</i>";
        }

        return EscapeHtml(content);
    }

    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}