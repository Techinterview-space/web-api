using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Telegram.GetTelegramUserSettings;

namespace Web.Api.Features.Telegram.UpdateTelegramUserSettings;

public class UpdateTelegramUserSettingsHandler
    : IRequestHandler<UpdateTelegramUserSettingsCommand, TelegramUserSettingsDto>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public UpdateTelegramUserSettingsHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<TelegramUserSettingsDto> Handle(
        UpdateTelegramUserSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _authorization.CurrentUserOrFailAsync(cancellationToken);
        var userSettings = await _context.TelegramUserSettings
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<TelegramUserSettings>(request.Id);

        if (!currentUser.Has(Role.Admin) && userSettings.UserId != currentUser.Id)
        {
            throw new NoPermissionsException("You don't have permissions to update this user settings.");
        }

        userSettings.Update(
            request.SendBotRegularStatsUpdates);

        await _context.SaveChangesAsync(cancellationToken);

        return new TelegramUserSettingsDto(userSettings);
    }
}