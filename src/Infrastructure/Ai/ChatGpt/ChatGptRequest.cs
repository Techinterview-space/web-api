using System.Text.Json.Serialization;

namespace Infrastructure.Ai.ChatGpt;

public record ChatGptRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("messages")]
    public List<ChatGptMessage> Messages { get; set; }

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;
}