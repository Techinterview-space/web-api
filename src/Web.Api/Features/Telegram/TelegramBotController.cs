using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Telegram.GetTelegramBotUsages;

namespace Web.Api.Features.Telegram;

[ApiController]
[Route("api/telegram-bot")]
public class TelegramBotController : ControllerBase
{
    private readonly IMediator _mediator;

    public TelegramBotController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("bot-usages")]
    public async Task<Pageable<TelegramBotUsageDto>> GetBotUsages(
        [FromQuery] GetTelegramBotUsagesQuery request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            request,
            cancellationToken);
    }
}