using System;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Emails;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Emails.ViewModels;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Emails;

[ApiController]
[Route("api/admin/email-previews")]
public class EmailPreviewController : ControllerBase
{
    private readonly IViewRenderer _viewRenderer;

    public EmailPreviewController(
        IViewRenderer viewRenderer)
    {
        _viewRenderer = viewRenderer;
    }

    [HttpGet("review-approved")]
    public async Task<EmailPreviewResponse> ReviewWasApproved()
    {
        var view = await _viewRenderer.RenderHtmlAsync(
            ReviewWasApprovedViewModel.ViewName,
            new ReviewWasApprovedViewModel("Company Name", Guid.NewGuid().ToString()));

        return new EmailPreviewResponse(view);
    }

    [HttpGet("review-rejected")]
    public async Task<EmailPreviewResponse> ReviewWasRejected()
    {
        var view = await _viewRenderer.RenderHtmlAsync(
            ReviewWasRejectedViewModel.ViewName,
            new ReviewWasRejectedViewModel("Company Name", Guid.NewGuid().ToString()));

        return new EmailPreviewResponse(view);
    }

    [HttpGet("salary-reminder")]
    public async Task<EmailPreviewResponse> UptodateSalaryReminder()
    {
        var view = await _viewRenderer.RenderHtmlAsync(
            SalaryUpdateReminderViewModel.ViewName,
            new SalaryUpdateReminderViewModel(Guid.NewGuid().ToString()));

        return new EmailPreviewResponse(view);
    }
}