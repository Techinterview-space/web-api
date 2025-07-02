using System.Text.Json.Serialization;

namespace Infrastructure.Ai.ChatGpt;

public record ChatGptMessage
{
    public ChatGptMessage()
    {
    }

    public ChatGptMessage(
        string role,
        string content)
    {
        Role = role;
        Content = content;
    }

    // "user", "assistant", or "system"
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}