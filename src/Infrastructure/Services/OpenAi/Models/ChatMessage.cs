using System.Text.Json.Serialization;

namespace Infrastructure.Services.OpenAi.Models;

public record ChatMessage
{
    public ChatMessage(
        string role,
        string content)
    {
        Role = role;
        Content = content;
    }

    public ChatMessage()
    {
    }

    [JsonPropertyName("role")]
    public string Role { get; set; } // "user", "assistant", or "system"

    [JsonPropertyName("content")]
    public string Content { get; set; }
}