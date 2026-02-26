using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Telegram.AddTelegramUserSettings;
using Web.Api.Features.Telegram.BotConfigurations;
using Web.Api.Features.Telegram.DeleteTelegramUserSettings;
using Web.Api.Features.Telegram.GetTelegramBotUsages;
using Web.Api.Features.Telegram.GetTelegramUserSettings;
using Web.Api.Features.Telegram.UpdateTelegramUserSettings;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Telegram;

[ApiController]
[Route("api/telegram-bot")]
[HasAnyRole(Role.Admin)]
public class TelegramBotController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public TelegramBotController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("configurations")]
    public async Task<List<TelegramBotConfigurationDto>> GetBotConfigurations(
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetTelegramBotConfigurationsHandler, GetTelegramBotConfigurationsQuery, List<TelegramBotConfigurationDto>>(
            new GetTelegramBotConfigurationsQuery(),
            cancellationToken);
    }

    [HttpGet("configurations/{id:guid}")]
    public async Task<TelegramBotConfigurationDto> GetBotConfiguration(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetTelegramBotConfigurationByIdHandler, GetTelegramBotConfigurationByIdQuery, TelegramBotConfigurationDto>(
            new GetTelegramBotConfigurationByIdQuery(id),
            cancellationToken);
    }

    [HttpPost("configurations")]
    public async Task<TelegramBotConfigurationDto> CreateBotConfiguration(
        [FromBody] CreateTelegramBotConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<CreateTelegramBotConfigurationHandler, CreateTelegramBotConfigurationRequest, TelegramBotConfigurationDto>(
            request,
            cancellationToken);
    }

    [HttpPut("configurations/{id:guid}")]
    public async Task<TelegramBotConfigurationDto> UpdateBotConfiguration(
        [FromRoute] Guid id,
        [FromBody] UpdateTelegramBotConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<UpdateTelegramBotConfigurationHandler, UpdateTelegramBotConfigurationCommand, TelegramBotConfigurationDto>(
            new UpdateTelegramBotConfigurationCommand(id, request),
            cancellationToken);
    }

    [HttpGet("bot-usages")]
    public async Task<Pageable<TelegramBotUsageDto>> GetBotUsages(
        [FromQuery] GetTelegramBotUsagesQuery request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetTelegramBotUsagesHandler, GetTelegramBotUsagesQuery, Pageable<TelegramBotUsageDto>>(
            request,
            cancellationToken);
    }

    [HttpGet("bot-user-settings")]
    public async Task<Pageable<TelegramUserSettingsDto>> GetTelegramUserSettings(
        [FromQuery] GetTelegramUserSettingsQuery request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetTelegramUserSettingsHandler, GetTelegramUserSettingsQuery, Pageable<TelegramUserSettingsDto>>(
            request,
            cancellationToken);
    }

    [HttpPost("bot-user-settings")]
    public async Task<TelegramUserSettingsDto> AddTelegramUserSettings(
        AddTelegramUserSettingsRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<AddTelegramUserSettingsHandler, AddTelegramUserSettingsCommand, TelegramUserSettingsDto>(
            new AddTelegramUserSettingsCommand(request),
            cancellationToken);
    }

    [HttpPut("bot-user-settings/{id:guid}")]
    public async Task<TelegramUserSettingsDto> UpdateTelegramUserSettings(
        [FromRoute] Guid id,
        UpdateTelegramUserSettingsBody request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<UpdateTelegramUserSettingsHandler, UpdateTelegramUserSettingsCommand, TelegramUserSettingsDto>(
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
        await _serviceProvider.HandleBy<DeleteTelegramUserSettingsHandler, DeleteTelegramUserSettingsCommand, Nothing>(
            new DeleteTelegramUserSettingsCommand(id),
            cancellationToken);

        return NoContent();
    }
}