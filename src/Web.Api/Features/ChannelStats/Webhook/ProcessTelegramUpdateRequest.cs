namespace Web.Api.Features.ChannelStats.Webhook;

public record ProcessTelegramUpdateRequest
{
    public ProcessTelegramUpdateRequest(
        long updateId,
        string payloadJson)
    {
        UpdateId = updateId;
        PayloadJson = payloadJson;
    }

    public long UpdateId { get; init; }

    public string PayloadJson { get; init; }
}
