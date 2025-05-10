using System.Collections.Generic;

namespace Web.Api.Features.Dashboard.Dtos;

public record TelegramInlineUsagesData
{
    public TelegramInlineUsagesData(
        List<TelegramInlineUsageItem> telegramInlineUsages)
    {
        Source = telegramInlineUsages;
    }

    public List<TelegramInlineUsageItem> Source { get; }
}