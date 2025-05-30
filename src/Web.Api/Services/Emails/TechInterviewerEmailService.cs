using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Infrastructure.Emails;
using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Infrastructure.Services.Global;
using Infrastructure.Services.Html;
using Infrastructure.Services.PDF.Interviews;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Web.Api.Features.Emails.ViewModels;

namespace Web.Api.Services.Emails;

public class TechInterviewerEmailService : ITechinterviewEmailService
{
    private readonly IHostEnvironment _env;
    private readonly IGlobal _global;
    private readonly IMarkdownToHtmlGenerator _html;
    private readonly IViewRenderer _viewRenderer;
    private readonly IEmailApiSender _emailApiSender;
    private readonly ILogger<TechInterviewerEmailService> _logger;

    public TechInterviewerEmailService(
        IHostEnvironment env,
        IGlobal global,
        IMarkdownToHtmlGenerator html,
        IViewRenderer viewRenderer,
        IEmailApiSender emailApiSender,
        ILogger<TechInterviewerEmailService> logger)
    {
        _env = env;
        _global = global;
        _html = html;
        _viewRenderer = viewRenderer;
        _emailApiSender = emailApiSender;
        _logger = logger;
    }

    public async Task CompanyReviewWasApprovedAsync(
        User user,
        string companyName,
        CancellationToken cancellationToken)
    {
        if (ShouldSkipEmailSending(user))
        {
            return;
        }

        var view = await _viewRenderer.RenderHtmlAsync(
            ReviewWasApprovedViewModel.ViewName,
            new ReviewWasApprovedViewModel(companyName, user.UniqueToken));

        await _emailApiSender.SendAsync(
            new EmailContent(
                _global.NoReplyEmail,
                "Ваш отзыв был одобрен",
                view,
                new List<string>
                {
                    user.Email,
                }),
            cancellationToken);
    }

    public async Task CompanyReviewWasRejectedAsync(
        User user,
        string companyName,
        CancellationToken cancellationToken)
    {
        if (ShouldSkipEmailSending(user))
        {
            return;
        }

        var view = await _viewRenderer.RenderHtmlAsync(
            ReviewWasRejectedViewModel.ViewName,
            new ReviewWasRejectedViewModel(companyName, user.UniqueToken));

        await _emailApiSender.SendAsync(
            new EmailContent(
                _global.NoReplyEmail,
                "Ваш отзыв был отклонен",
                view,
                new List<string>
                {
                    user.Email,
                }),
            cancellationToken);
    }

    private bool ShouldSkipEmailSending(
        User user)
    {
        if (!user.IsGoogleAuth() && !user.IsGithubAuth())
        {
            _logger.LogWarning(
                "Email about approval was not sent to user {UserId} with email {Email} because they are not authenticated via Google or GitHub.",
                user.Id,
                user.Email);

            return true;
        }

        if (user.UnsubscribeMeFromAll)
        {
            _logger.LogWarning(
                "Email about approval was not sent to user {UserId} with email {Email} because the user has unsubscribed from all emails.",
                user.Id,
                user.Email);

            return true;
        }

        if (user.UniqueToken == null)
        {
            _logger.LogWarning(
                "User {UserId} with email {Email} has no unsubscribe token. Skipping email.",
                user.Id,
                user.Email);

            return true;
        }

        return false;
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