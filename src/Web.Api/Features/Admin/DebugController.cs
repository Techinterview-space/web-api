using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Features.Admin;

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IEmailSender _emailSender;

    public DebugController(
        IEmailService emailService,
        IEmailSender emailSender)
    {
        _emailService = emailService;
        _emailSender = emailSender;
    }

    [HttpPost("emails/direct")]
    public async Task<IActionResult> SendDirectEmailAsync([FromBody] DirectEmailSendRequest request)
    {
        await _emailSender.SendAsync(_emailService.Prepare(new EmailSendRequest(
            "Hi there",
            request.Email,
            request.Body)));

        return Ok();
    }

    [HttpPost("emails/show-body")]
    public IActionResult ShowEmailBody([FromBody] DirectEmailSendRequest request)
    {
        return Ok(_emailService.Prepare(new EmailSendRequest(
            "Hi there",
            request.Email,
            request.Body)));
    }

    public record DirectEmailSendRequest
    {
        [Required]
        public string Email { get; init; }

        public string Body { get; init; }
    }
}