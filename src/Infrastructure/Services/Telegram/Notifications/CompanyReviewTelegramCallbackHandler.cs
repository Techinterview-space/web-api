using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Database;
using Infrastructure.Emails.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.Services.Telegram.Notifications;

public class CompanyReviewTelegramCallbackHandler : ICompanyReviewTelegramCallbackHandler
{
    private const string ApprovePrefix = "review_approve:";
    private const string RejectPrefix = "review_reject:";

    private readonly DatabaseContext _context;
    private readonly ITechinterviewEmailService _emailService;
    private readonly ILogger<CompanyReviewTelegramCallbackHandler> _logger;

    public CompanyReviewTelegramCallbackHandler(
        DatabaseContext context,
        ITechinterviewEmailService emailService,
        ILogger<CompanyReviewTelegramCallbackHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> TryHandleCallbackAsync(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken = default)
    {
        var data = callbackQuery.Data;
        if (string.IsNullOrEmpty(data))
        {
            return false;
        }

        if (!data.StartsWith(ApprovePrefix) && !data.StartsWith(RejectPrefix))
        {
            return false;
        }

        var isApprove = data.StartsWith(ApprovePrefix);
        var reviewIdString = isApprove
            ? data[ApprovePrefix.Length..]
            : data[RejectPrefix.Length..];

        if (!Guid.TryParse(reviewIdString, out var reviewId))
        {
            await botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                "Неверный формат ID отзыва",
                cancellationToken: cancellationToken);

            return true;
        }

        var chatId = callbackQuery.Message?.Chat.Id ?? callbackQuery.From.Id;
        var isAdmin = await IsUserAdminAsync(chatId, cancellationToken);

        if (!isAdmin)
        {
            await botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                "У вас нет прав для выполнения этого действия",
                showAlert: true,
                cancellationToken: cancellationToken);

            return true;
        }

        try
        {
            if (isApprove)
            {
                await ApproveReviewAsync(reviewId, cancellationToken);
                await UpdateMessageWithResultAsync(
                    botClient,
                    callbackQuery,
                    "✅ Отзыв одобрен",
                    cancellationToken);
            }
            else
            {
                await RejectReviewAsync(reviewId, cancellationToken);
                await UpdateMessageWithResultAsync(
                    botClient,
                    callbackQuery,
                    "❌ Отзыв отклонен",
                    cancellationToken);
            }

            await botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                isApprove ? "Отзыв одобрен" : "Отзыв отклонен",
                cancellationToken: cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing callback for review {ReviewId}",
                reviewId);

            await botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                $"Ошибка: {ex.Message}",
                showAlert: true,
                cancellationToken: cancellationToken);

            return true;
        }
    }

    private async Task<bool> IsUserAdminAsync(
        long chatId,
        CancellationToken cancellationToken)
    {
        return await _context.TelegramUserSettings
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .AnyAsync(
                x => x.ChatId == chatId &&
                     x.User.UserRoles.Any(r => r.RoleId == Role.Admin),
                cancellationToken);
    }

    private async Task ApproveReviewAsync(
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        var review = await _context.CompanyReviews
            .Include(x => x.Company)
            .ThenInclude(x => x.Reviews)
            .Include(x => x.Company)
            .ThenInclude(x => x.RatingHistory)
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Id == reviewId,
                cancellationToken);

        if (review == null)
        {
            throw new InvalidOperationException("Отзыв не найден");
        }

        if (review.ApprovedAt != null)
        {
            throw new InvalidOperationException("Отзыв уже одобрен");
        }

        review.Approve();
        review.User?.GenerateNewEmailUnsubscribeTokenIfNecessary();

        await _context.SaveChangesAsync(cancellationToken);

        if (review.User != null)
        {
            await _emailService.CompanyReviewWasApprovedAsync(
                review.User,
                review.Company.Name,
                cancellationToken);

            await _context.SaveAsync(
                new UserEmail(
                    "Отзыв был одобрен",
                    UserEmailType.CompanyReviewNotification,
                    review.User),
                cancellationToken);
        }

        _logger.LogInformation(
            "Review {ReviewId} approved via Telegram callback",
            reviewId);
    }

    private async Task RejectReviewAsync(
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        var review = await _context.CompanyReviews
            .Include(x => x.Company)
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Id == reviewId,
                cancellationToken);

        if (review == null)
        {
            throw new InvalidOperationException("Отзыв не найден");
        }

        var companyName = review.Company.Name;
        var user = review.User;

        _context.Remove(review);
        user?.GenerateNewEmailUnsubscribeTokenIfNecessary();

        await _context.SaveChangesAsync(cancellationToken);

        if (user != null)
        {
            await _emailService.CompanyReviewWasRejectedAsync(
                user,
                companyName,
                cancellationToken);

            await _context.SaveAsync(
                new UserEmail(
                    "Отзыв был отклонен",
                    UserEmailType.CompanyReviewNotification,
                    user),
                cancellationToken);
        }

        _logger.LogInformation(
            "Review {ReviewId} rejected via Telegram callback",
            reviewId);
    }

    private static async Task UpdateMessageWithResultAsync(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        string resultText,
        CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null)
        {
            return;
        }

        var originalText = callbackQuery.Message.Text ?? string.Empty;
        var updatedText = $"{originalText}\n\n<b>{resultText}</b>";

        await botClient.EditMessageText(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            updatedText,
            parseMode: ParseMode.Html,
            replyMarkup: null,
            cancellationToken: cancellationToken);
    }
}
