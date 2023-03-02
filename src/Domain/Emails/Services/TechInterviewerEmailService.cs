using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain.Emails.Requests;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Domain.Services.Global;
using Domain.Services.Html;
using Domain.Services.MD;
using EmailService.Integration.Core;
using EmailService.Integration.Core.Clients;
using EmailService.Integration.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Domain.Emails.Services;

public class TechInterviewerEmailService : IEmailService
{
    private readonly IEmailSender _sender;
    private readonly IWebHostEnvironment _env;
    private readonly IGlobal _global;
    private readonly ITechInterviewHtmlGenerator _html;

    public TechInterviewerEmailService(
        IEmailSender sender,
        IWebHostEnvironment env,
        IGlobal global,
        ITechInterviewHtmlGenerator html)
    {
        _sender = sender;
        _env = env;
        _global = global;
        _html = html;
    }

    public async Task SendEmailAsync(EmailSendRequest emailContent)
    {
        if (_global.EnableEmailPublishing)
        {
            await _sender.SendAsync(Prepare(emailContent));
        }
    }

    public Task InvitationAsync(
        Organization organization, User invitedPerson, User inviter)
    {
        return SendEmailAsync(new OrganizationUserInviteRequest(
            _global,
            organization,
            invitedPerson,
            inviter));
    }

    public Task InvitationAcceptedAsync(Organization organization, User invitedPerson, User inviter)
    {
        return SendEmailAsync(new InvitationAcceptedEmailRequest(
            _global,
            invitedPerson,
            inviter,
            organization));
    }

    public Task InvitationDeclinedAsync(Organization organization, User invitedPerson, User inviter)
    {
        return SendEmailAsync(new InvitationDeclinedEmailRequest(
            _global,
            invitedPerson,
            inviter,
            organization));
    }

    public EmailContent Prepare(EmailSendRequest emailContent) =>
        new EmailContent(
            _global.NoReplyEmail,
            PrepareSubject(emailContent.Subject),
            PrepareBody(emailContent.Body),
            emailContent.Recipients,
            emailContent.Cc,
            PrepareHiddenCc(emailContent.HiddenCc));

    private string PrepareSubject(
        string subject)
    {
        return _env.IsProduction() ? subject : $"[{_env.EnvironmentName}] {subject}";
    }

    private string PrepareBody(
        string body)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(body);
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(MarkdownItems.Line());
        stringBuilder.AppendLine(
            MarkdownItems.Italic(
                "This email is automatically generated. Please do not reply to this email."));
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(MarkdownItems.Italic(_global.AppName));
        stringBuilder.AppendLine();

        if (!_env.IsProduction())
        {
            stringBuilder.AppendLine(MarkdownItems.Italic(_global.AppVersion));
            stringBuilder.AppendLine();
        }

        return new EmailHtmlTemplate(_html.FromMarkdown(stringBuilder.ToString())).ToString();
    }

    private ICollection<string> PrepareHiddenCc(
        ICollection<string> hiddenCc)
    {
        if (_env.IsProduction() ||
            !_global.AddDevEmailsToHiddenCc ||
            _global.DeveloperEmails.Count == 0)
        {
            return hiddenCc;
        }

        foreach (var devEmail in _global.DeveloperEmails)
        {
            hiddenCc.Add(devEmail);
        }

        return hiddenCc;
    }
}