using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Emails;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Emails.ViewModels;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Emails;

[ApiController]
[Route("api/admin/email-previews")]
[HasAnyRole(Role.Admin)]
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
            new ReviewWasApprovedViewModel("Company Name"));

        return new EmailPreviewResponse(view);
    }
}