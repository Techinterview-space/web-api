using Domain.Entities.Companies;

namespace Infrastructure.Services.Telegram.Notifications;

public interface ITelegramAdminNotificationService
{
    Task NotifyAboutNewCompanyReviewAsync(
        CompanyReview review,
        Company company,
        CancellationToken cancellationToken = default);
}