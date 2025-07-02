using System.Text.Json.Serialization;

namespace Infrastructure.Services.OpenAi.Models;

public record ClaudeChatContentItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }
}