﻿using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.DeleteTelegramUserSettings;

public class DeleteTelegramUserSettingsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<DeleteTelegramUserSettingsCommand, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public DeleteTelegramUserSettingsHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Nothing> Handle(
        DeleteTelegramUserSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);
        var userSettings = await _context.TelegramUserSettings
                               .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                           ?? throw NotFoundException.CreateFromEntity<TelegramUserSettings>(request.Id);

        if (!currentUser.Has(Role.Admin) && userSettings.UserId != currentUser.Id)
        {
            throw new NoPermissionsException("You don't have permissions to update this user settings.");
        }

        _context.TelegramUserSettings.Remove(userSettings);
        await _context.SaveChangesAsync(cancellationToken);

        return Nothing.Value;
    }
}