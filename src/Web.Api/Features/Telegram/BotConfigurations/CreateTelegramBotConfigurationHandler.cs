using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Telegram;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.BotConfigurations;

public class CreateTelegramBotConfigurationHandler
    : Infrastructure.Services.Mediator.IRequestHandler<CreateTelegramBotConfigurationRequest, TelegramBotConfigurationDto>
{
    private readonly DatabaseContext _context;
    private readonly ITelegramBotConfigurationService _configurationService;

    public CreateTelegramBotConfigurationHandler(
        DatabaseContext context,
        ITelegramBotConfigurationService configurationService)
    {
        _context = context;
        _configurationService = configurationService;
    }

    public async Task<TelegramBotConfigurationDto> Handle(
        CreateTelegramBotConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.BotType == TelegramBotType.Undefined)
        {
            throw new BadRequestException("BotType is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new BadRequestException("DisplayName is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new BadRequestException("Token is required when creating a new bot configuration.");
        }

        if (await _context.TelegramBotConfigurations.AnyAsync(
                x => x.BotType == request.BotType,
                cancellationToken))
        {
            throw new BadRequestException(
                $"Telegram bot configuration for {request.BotType} already exists.");
        }

        var entity = await _context.SaveAsync(
            new TelegramBotConfiguration(
                request.BotType,
                request.DisplayName,
                request.Token,
                request.IsEnabled,
                request.BotUsername),
            cancellationToken);

        _configurationService.InvalidateCache(request.BotType);

        return new TelegramBotConfigurationDto(entity);
    }
}
