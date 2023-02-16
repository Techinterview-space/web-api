using System.Text;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Domain.Services.Global;
using Domain.Services.MD;

namespace Domain.Emails.Requests;

public record OrganizationUserInviteRequest : EmailSendRequest
{
    public OrganizationUserInviteRequest(
        IGlobal global,
        Organization organization,
        User invitedPerson,
        User inviter)
        : base(
            subject: $"You are invited to join {organization.Name}",
            recipient: invitedPerson.Email,
            body: PrepareBody(
                global,
                organization,
                inviter))
    {
    }

    private static string PrepareBody(
        IGlobal global,
        Organization organization,
        User inviter)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("You are invited to join " + MarkdownItems.Bold(organization.Name));
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Inviter: " + inviter.Fullname);
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(
            "Please " +
            MarkdownItems.Link(global.ProfileWebLink(), "go to the profile page") +
            " to accept or decline the invitation.");
        stringBuilder.AppendLine();

        return stringBuilder.ToString();
    }
}