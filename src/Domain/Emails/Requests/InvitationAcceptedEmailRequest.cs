using System.Text;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Domain.Services.Global;
using Domain.Services.MD;

namespace Domain.Emails.Requests;

public record InvitationAcceptedEmailRequest : EmailSendRequest
{
    public InvitationAcceptedEmailRequest(
        IGlobal global,
        User invitedPerson,
        User inviter,
        Organization organization)
        : base(
            subject: $"The user {invitedPerson.Fullname} has accepted your invitation",
            recipient: inviter.Email,
            body: PrepareBody(
                global,
                inviter,
                organization))
    {
    }

    private static string PrepareBody(
        IGlobal global,
        User invitedPerson,
        Organization organization)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"The user {MarkdownItems.Bold(invitedPerson.Fullname)} has accepted your invitation.");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(
            $"Organization: {MarkdownItems.Link(global.OrganizationWebLink(organization.Id), organization.Name)}");
        stringBuilder.AppendLine();

        return stringBuilder.ToString();
    }
}