﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.GetTelegramBotUsages;

public class GetTelegramBotUsagesHandler : Infrastructure.Services.Mediator.IRequestHandler<GetTelegramBotUsagesQuery, Pageable<TelegramBotUsageDto>>
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
        return await _context.TelegramBotUsages
            .OrderByDescending(x => x.UsageCount)
            .AsNoTracking()
            .Select(x => new TelegramBotUsageDto
            {
                Id = x.Id,
                ChatId = x.ChatId,
                ReceivedMessageText = x.ReceivedMessageText,
                UsageCount = x.UsageCount,
                Username = x.Username,
                UsageType = x.UsageType,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            })
            .AsPaginatedAsync(request, cancellationToken);
    }
}