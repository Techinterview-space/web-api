using System.Text.Json.Serialization;

namespace Infrastructure.Ai.Claude;

public record ClaudeChatContentItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }
}