using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Telegram;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.BotConfigurations;

public class UpdateTelegramBotConfigurationHandler
    : Infrastructure.Services.Mediator.IRequestHandler<UpdateTelegramBotConfigurationCommand, TelegramBotConfigurationDto>
{
    private readonly DatabaseContext _context;
    private readonly ITelegramBotConfigurationService _configurationService;

    public UpdateTelegramBotConfigurationHandler(
        DatabaseContext context,
        ITelegramBotConfigurationService configurationService)
    {
        _context = context;
        _configurationService = configurationService;
    }

    public async Task<TelegramBotConfigurationDto> Handle(
        UpdateTelegramBotConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new BadRequestException("DisplayName is required.");
        }

        var entity = await _context.TelegramBotConfigurations
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<TelegramBotConfiguration>(request.Id);

        entity.Update(
            request.DisplayName,
            request.IsEnabled,
            request.BotUsername,
            request.Token);

        await _context.TrySaveChangesAsync(cancellationToken);

        _configurationService.InvalidateCache(entity.BotType);

        return new TelegramBotConfigurationDto(entity);
    }
}
