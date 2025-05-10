using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.GetTelegramInlineUsageStats;

public class GetTelegramInlineUsageStatsHandler
    : IRequestHandler<GetTelegramInlineUsageStatsQuery, TelegramInlineUsagesData>
{
    private readonly DatabaseContext _context;

    public GetTelegramInlineUsageStatsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<TelegramInlineUsagesData> Handle(
        GetTelegramInlineUsageStatsQuery request,
        CancellationToken cancellationToken)
    {
        var yearAgo = DateTimeOffset.UtcNow.AddYears(-1);
        var telegramBotInlineUsages = await _context.TelegramInlineReplies
            .Where(x => x.CreatedAt >= yearAgo)
            .Select(x => new TelegramInlineUsageSourceItem
            {
                CreatedAt = x.CreatedAt,
                Username = x.Username,
                ChatId = x.ChatId,
                ChatName = x.ChatName
            })
            .ToListAsync(cancellationToken);

        return new TelegramInlineUsagesData(telegramBotInlineUsages);
    }
}