using System.Text.Json.Serialization;

namespace Infrastructure.Services.OpenAi.Models;

public record Choice
{
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; }
}