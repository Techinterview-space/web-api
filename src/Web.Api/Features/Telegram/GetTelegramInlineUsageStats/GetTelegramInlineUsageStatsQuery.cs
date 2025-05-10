using MediatR;

namespace Web.Api.Features.Telegram.GetTelegramInlineUsageStats;

public record GetTelegramInlineUsageStatsQuery
    : IRequest<TelegramInlineUsagesData>;