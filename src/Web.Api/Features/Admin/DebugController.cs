using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Emails;
using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Infrastructure.Services.Global;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Emails.ViewModels;

namespace Web.Api.Features.Admin;

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    private readonly IEmailApiSender _emailApiSender;
    private readonly IViewRenderer _viewRenderer;
    private readonly IGlobal _global;

    public DebugController(
        IEmailApiSender emailApiSender,
        IViewRenderer viewRenderer,
        IGlobal global)
    {
        _emailApiSender = emailApiSender;
        _viewRenderer = viewRenderer;
        _global = global;
    }

    [HttpPost("emails/direct")]
    public async Task<IActionResult> SendDirectEmailAsync(
        [FromBody] DirectEmailSendRequest request,
        CancellationToken cancellationToken)
    {
        var view = await _viewRenderer.RenderHtmlAsync(
            "/Views/Emails/SimpleEmail.cshtml",
            new SimpleEmailViewModel(
                request.Body,
                request.BodyTitle));

        await _emailApiSender.SendAsync(
            new EmailContent(
                _global.NoReplyEmail,
                "Your email was verified",
                view,
                new List<string>
                {
                    request.Email,
                }),
            cancellationToken);

        return Ok();
    }

    public record DirectEmailSendRequest
    {
        [Required]
        public string Email { get; init; }

        public string BodyTitle { get; init; }

        public string Body { get; init; }
    }
}