using System.Text.Json.Serialization;

namespace Infrastructure.Services.OpenAi.Models;

public record AiChoice
{
    public AiChoice()
    {
    }

    public AiChoice(
        string role,
        ClaudeChatContentItem claudeContent)
    {
        Message = new ChatMessage(
            role,
            claudeContent.Text);
    }

    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; }
}