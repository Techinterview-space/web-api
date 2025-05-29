using Infrastructure.Emails.Contracts.Requests;

namespace Infrastructure.Emails.Contracts;

public interface ITechinterviewEmailService
{
    Task CompanyReviewWasApprovedAsync(
        string userEmail,
        string companyName,
        CancellationToken cancellationToken);

    Task CompanyReviewWasRejectedAsync(
        string userEmail,
        string companyName,
        CancellationToken cancellationToken);

    EmailContent Prepare(
        SendGridEmailSendRequest emailContent);
}