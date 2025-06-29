using System.Text.Json.Serialization;

namespace Infrastructure.Services.OpenAi.Models;

public record ChatResponse
{
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; }
}