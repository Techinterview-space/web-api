using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using MediatR;

namespace Web.Api.Features.Telegram.GetTelegramUserSettings;

public class GetTelegramUserSettingsHandler
    : IRequestHandler<GetTelegramUserSettingsQuery, Pageable<TelegramUserSettingsDto>>
{
    private readonly DatabaseContext _context;

    public GetTelegramUserSettingsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<TelegramUserSettingsDto>> Handle(
        GetTelegramUserSettingsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.TelegramUserSettings
            .OrderBy(x => x.CreatedAt)
            .Select(TelegramUserSettingsDto.Transform)
            .AsPaginatedAsync(request, cancellationToken);
    }
}