using System;

namespace Web.Api.Features.Dashboard.Dtos;

public record TelegramInlineUsageItem
{
    public DateTime CreatedAt { get; init; }

    public string Username { get; init; }
}