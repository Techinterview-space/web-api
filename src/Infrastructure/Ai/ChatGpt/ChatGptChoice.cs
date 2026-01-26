using System.Text.Json.Serialization;

namespace Infrastructure.Ai.ChatGpt;

public record ChatGptChoice
{
    [JsonPropertyName("message")]
    public ChatGptMessage Message { get; set; }
}