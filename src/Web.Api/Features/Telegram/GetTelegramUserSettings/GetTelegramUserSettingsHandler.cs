using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.GetTelegramUserSettings;

public class GetTelegramUserSettingsHandler
    : IRequestHandler<GetTelegramUserSettingsQuery, List<TelegramUserSettingsDto>>
{
    private readonly DatabaseContext _context;

    public GetTelegramUserSettingsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<TelegramUserSettingsDto>> Handle(
        GetTelegramUserSettingsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.TelegramUserSettings
            .Select(TelegramUserSettingsDto.Transform)
            .ToListAsync(cancellationToken);
    }
}