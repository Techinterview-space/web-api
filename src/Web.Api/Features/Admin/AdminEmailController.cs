using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Admin;

[ApiController]
[Route("api/admin/emails")]
[HasAnyRole(Role.Admin)]
public class AdminEmailController : ControllerBase
{
    private readonly IEmailApiSender _emailApiSender;

    public AdminEmailController(IEmailApiSender emailApiSender)
    {
        _emailApiSender = emailApiSender;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendCustomEmailAsync(
        [FromBody] SendCustomEmailRequest request,
        CancellationToken cancellationToken)
    {
        var recipients = new List<string> { request.To };

        var cc = !string.IsNullOrWhiteSpace(request.Cc)
            ? new List<string> { request.Cc }
            : new List<string>();

        await _emailApiSender.SendAsync(
            new EmailContent(
                request.From,
                request.Subject,
                request.HtmlBody,
                recipients,
                cc),
            cancellationToken);

        return Ok(new SendCustomEmailResponse
        {
            Success = true,
            Message = "Email sent successfully"
        });
    }

    public record SendCustomEmailRequest
    {
        [Required]
        [EmailAddress]
        public string To { get; init; }

        [EmailAddress]
        public string Cc { get; init; }

        [Required]
        [EmailAddress]
        public string From { get; init; }

        [Required]
        public string Subject { get; init; }

        [Required]
        public string HtmlBody { get; init; }
    }

    public record SendCustomEmailResponse
    {
        public bool Success { get; init; }

        public string Message { get; init; }
    }
}
