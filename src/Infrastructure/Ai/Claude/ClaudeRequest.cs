using System.Text.Json.Serialization;

namespace Infrastructure.Ai.Claude;

public record ClaudeRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("system")]
    public string SystemPrompt { get; set; }

    [JsonPropertyName("messages")]
    public List<ClaudeChatMessage> Messages { get; set; }
}