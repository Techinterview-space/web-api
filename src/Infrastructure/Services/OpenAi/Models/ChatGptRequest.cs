using System.Text.Json.Serialization;

namespace Infrastructure.Services.OpenAi.Models;

public record ChatGptRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; }

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;
}