using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.GetTelegramBotUsages;

public class GetTelegramBotUsagesHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetTelegramBotUsagesQuery, Pageable<TelegramBotUsageDto>>
{
    private readonly DatabaseContext _context;

    public GetTelegramBotUsagesHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<TelegramBotUsageDto>> Handle(
        GetTelegramBotUsagesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.SalariesBotMessages
            .AsNoTracking()
            .Select(x => new TelegramBotUsageDto
            {
                Id = x.Id,
                ChatId = x.ChatId,
                Username = x.Username,
                UsageType = x.UsageType,
                IsAdmin = x.IsAdmin,
                CreatedAt = x.CreatedAt,
            })
            .OrderByDescending(x => x.CreatedAt)
            .AsPaginatedAsync(request, cancellationToken);
    }
}