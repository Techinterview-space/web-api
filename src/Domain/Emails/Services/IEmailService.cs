using System.Threading.Tasks;
using Domain.Emails.Requests;
using Domain.Entities.Organizations;
using Domain.Entities.Users;

namespace Domain.Emails.Services;

public interface IEmailService
{
    Task SendEmailAsync(EmailSendRequest emailContent);

    Task InvitationAsync(
        Organization organization,
        User invitedPerson,
        User inviter);

    Task InvitationAcceptedAsync(
        Organization organization,
        User invitedPerson,
        User inviter);

    Task InvitationDeclinedAsync(
        Organization organization,
        User invitedPerson,
        User inviter);

    EmailContent Prepare(EmailSendRequest emailContent);
}