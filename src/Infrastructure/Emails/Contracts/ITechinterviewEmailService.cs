using Domain.Entities.Users;
using Infrastructure.Emails.Contracts.Requests;

namespace Infrastructure.Emails.Contracts;

public interface ITechinterviewEmailService
{
    Task<bool> CompanyReviewWasApprovedAsync(
        User user,
        string companyName,
        CancellationToken cancellationToken);

    Task<bool> CompanyReviewWasRejectedAsync(
        User user,
        string companyName,
        CancellationToken cancellationToken);

    Task<bool> SalaryUpdateReminderEmailAsync(
        User user,
        CancellationToken cancellationToken);

    EmailContent Prepare(
        SendGridEmailSendRequest emailContent);
}