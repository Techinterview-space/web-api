using System.Text.Json.Serialization;

namespace Infrastructure.Services.OpenAi.Models;

public record ClaudeRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("system")]
    public string SystemPrompt { get; set; }

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; }
}