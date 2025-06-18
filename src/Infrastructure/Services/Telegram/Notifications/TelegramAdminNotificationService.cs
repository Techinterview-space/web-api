using Domain.Entities.Companies;
using Domain.Enums;
using Infrastructure.Database;
using Infrastructure.Services.Global;
using Infrastructure.Services.Telegram.Salaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

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

        var message = $"Новый отзыв о компании {company.Name}:\n" +
                      $"Рейтинг: {review.TotalRating}\n" +
                      $"Плюсы: {review.Pros?.Length}\n" +
                      $"Минусы: {review.Cons?.Length}\n\n" +
                      $"Ссылка: {_globalSettings.FrontendBaseUrl}/admin/companies/reviews-to-approve";

        foreach (var admin in admins)
        {
            await botClient.SendMessage(
                admin.ChatId,
                message,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Sent notification to admin {UserEmail} ({ChatId}) about new review {ReviewId}",
                admin.User.Email,
                admin.ChatId,
                review.Id);
        }
    }
}