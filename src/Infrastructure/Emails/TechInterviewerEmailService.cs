using System.Text;
using Domain.Services.Global;
using Domain.Services.Html;
using Domain.Services.MD;
using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Emails;

public class TechInterviewerEmailService : IEmailService
{
    private readonly IHostEnvironment _env;
    private readonly IGlobal _global;
    private readonly ITechInterviewHtmlGenerator _html;

    public TechInterviewerEmailService(
        IHostEnvironment env,
        IGlobal global,
        ITechInterviewHtmlGenerator html)
    {
        _env = env;
        _global = global;
        _html = html;
    }

    public EmailContent Prepare(
        EmailSendRequest emailContent) =>
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