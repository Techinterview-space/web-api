using Infrastructure.Ai.ChatGpt;
using Infrastructure.Ai.Claude;

namespace Infrastructure.Ai;

public record AiChoice
{
    public AiChoice(
        ChatGptMessage chatGptMessage)
    {
        Role = chatGptMessage.Role;
        Content = chatGptMessage.Content;
    }

    public AiChoice(
        string role,
        ClaudeChatContentItem claudeContent)
    {
        Role = role;
        Content = claudeContent.Text;
    }

    public string Role { get; }

    public string Content { get; }
}