using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Domain.Entities.Users;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Telegram.GetTelegramUserSettings;

namespace Web.Api.Features.Telegram.AddTelegramUserSettings;

public class AddTelegramUserSettingsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<AddTelegramUserSettingsCommand, TelegramUserSettingsDto>
{
    private readonly DatabaseContext _context;

    public AddTelegramUserSettingsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<TelegramUserSettingsDto> Handle(
        AddTelegramUserSettingsCommand request,
        CancellationToken cancellationToken)
    {
        if (await _context.TelegramUserSettings.AnyAsync(
                x =>
                    x.ChatId == request.ChatId ||
                    x.Username == request.Username ||
                    x.UserId == request.UserId,
                cancellationToken))
        {
            throw new BadRequestException(
                "Telegram user settings with the chat id or username or user id already exist.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<User>(request.UserId);

        var telegramUserSettings = await _context.SaveAsync(
            new TelegramUserSettings(
                request.Username,
                request.ChatId,
                user,
                request.SendBotRegularStatsUpdates),
            cancellationToken);

        return new TelegramUserSettingsDto(telegramUserSettings);
    }
}