using System.Text.Json.Serialization;

namespace Infrastructure.Ai.Claude;

public record ClaudeChatMessage
{
    public ClaudeChatMessage(
        string role,
        string content)
    {
        Role = role;
        Content = content;
    }

    public ClaudeChatMessage()
    {
    }

    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}