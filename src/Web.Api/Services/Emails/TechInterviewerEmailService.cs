using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Emails;
using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Infrastructure.Services.Global;
using Infrastructure.Services.Html;
using Infrastructure.Services.PDF.Interviews;
using Microsoft.Extensions.Hosting;
using Web.Api.Features.Emails.ViewModels;

namespace Web.Api.Services.Emails;

public class TechInterviewerEmailService : ITechinterviewEmailService
{
    private readonly IHostEnvironment _env;
    private readonly IGlobal _global;
    private readonly IMarkdownToHtmlGenerator _html;
    private readonly IViewRenderer _viewRenderer;
    private readonly IEmailApiSender _emailApiSender;

    public TechInterviewerEmailService(
        IHostEnvironment env,
        IGlobal global,
        IMarkdownToHtmlGenerator html,
        IViewRenderer viewRenderer,
        IEmailApiSender emailApiSender)
    {
        _env = env;
        _global = global;
        _html = html;
        _viewRenderer = viewRenderer;
        _emailApiSender = emailApiSender;
    }

    public async Task CompanyReviewWasApprovedAsync(
        string userEmail,
        string companyName,
        CancellationToken cancellationToken)
    {
        var view = await _viewRenderer.RenderHtmlAsync(
            ReviewWasApprovedViewModel.ViewName,
            new ReviewWasApprovedViewModel(companyName));

        await _emailApiSender.SendAsync(
            new EmailContent(
                _global.NoReplyEmail,
                "Ваш отзыв был одобрен",
                view,
                new List<string>
                {
                    userEmail,
                }),
            cancellationToken);
    }

    public async Task CompanyReviewWasRejectedAsync(
        string userEmail,
        string companyName,
        CancellationToken cancellationToken)
    {
        var view = await _viewRenderer.RenderHtmlAsync(
            ReviewWasRejectedViewModel.ViewName,
            new ReviewWasRejectedViewModel(companyName));

        await _emailApiSender.SendAsync(
            new EmailContent(
                _global.NoReplyEmail,
                "Ваш отзыв был отклонен",
                view,
                new List<string>
                {
                    userEmail,
                }),
            cancellationToken);
    }

    public EmailContent Prepare(
        SendGridEmailSendRequest emailContent) =>
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