using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Telegram.AddTelegramUserSettings;
using Web.Api.Features.Telegram.DeleteTelegramUserSettings;
using Web.Api.Features.Telegram.GetTelegramBotUsages;
using Web.Api.Features.Telegram.GetTelegramInlineUsageStats;
using Web.Api.Features.Telegram.GetTelegramUserSettings;
using Web.Api.Features.Telegram.UpdateTelegramUserSettings;
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
    public async Task<Pageable<TelegramUserSettingsDto>> GetTelegramUserSettings(
        [FromQuery] GetTelegramUserSettingsQuery request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            request,
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

    [HttpPut("bot-user-settings/{id:guid}")]
    public async Task<TelegramUserSettingsDto> UpdateTelegramUserSettings(
        [FromRoute] Guid id,
        UpdateTelegramUserSettingsBody request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new UpdateTelegramUserSettingsCommand(
                id,
                request),
            cancellationToken);
    }

    [HttpDelete("bot-user-settings/{id:guid}")]
    public async Task<IActionResult> DeleteTelegramUserSettings(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteTelegramUserSettingsCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("inline-reply-stats")]
    public async Task<TelegramInlineUsagesData> GetTelegramInlineReplyStats(
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new GetTelegramInlineUsageStatsQuery(),
            cancellationToken);
    }
}