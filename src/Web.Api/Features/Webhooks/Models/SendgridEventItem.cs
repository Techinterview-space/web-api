using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Web.Api.Features.Webhooks.Models;

public record SendgridEventItem
{
    [JsonPropertyName("email")]
    public string Email { get; init; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; init; }

    [JsonPropertyName("smtp-id")]
    public string SmtpId { get; init; }

    [JsonPropertyName("event")]
    public string Event { get; init; }

    [JsonPropertyName("category")]
    public List<string> Category { get; init; }

    [JsonPropertyName("sg_event_id")]
    public string EventId { get; init; }

    [JsonPropertyName("sg_message_id")]
    public string MessageId { get; init; }

    [JsonPropertyName("ip")]
    public string IpAddress { get; init; }

    [JsonPropertyName("url")]
    public string Url { get; init; }
}