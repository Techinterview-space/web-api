using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Telegram.AddTelegramUserSettings;
using Web.Api.Features.Telegram.GetTelegramBotUsages;
using Web.Api.Features.Telegram.GetTelegramUserSettings;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Telegram;

[ApiController]
[Route("api/telegram-bot")]
[HasAnyRole(Role.Admin)]
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

    [HttpGet("bot-user-settings")]
    public async Task<List<TelegramUserSettingsDto>> GetTelegramUserSettings(
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new GetTelegramUserSettingsQuery(),
            cancellationToken);
    }

    [HttpPost("bot-user-settings")]
    public async Task<TelegramUserSettingsDto> AddTelegramUserSettings(
        AddTelegramUserSettingsRequest request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new AddTelegramUserSettingsCommand(request),
            cancellationToken);
    }
}