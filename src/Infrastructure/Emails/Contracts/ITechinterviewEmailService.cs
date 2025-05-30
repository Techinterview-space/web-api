using Domain.Entities.Users;
using Infrastructure.Emails.Contracts.Requests;

namespace Infrastructure.Emails.Contracts;

public interface ITechinterviewEmailService
{
    Task CompanyReviewWasApprovedAsync(
        User user,
        string companyName,
        CancellationToken cancellationToken);

    Task CompanyReviewWasRejectedAsync(
        User user,
        string companyName,
        CancellationToken cancellationToken);

    EmailContent Prepare(
        SendGridEmailSendRequest emailContent);
}