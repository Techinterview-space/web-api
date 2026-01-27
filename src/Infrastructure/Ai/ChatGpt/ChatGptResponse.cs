using System.Text.Json.Serialization;

namespace Infrastructure.Ai.ChatGpt;

public record ChatGptResponse
{
    [JsonPropertyName("choices")]
    public List<ChatGptChoice> Choices { get; set; }
}