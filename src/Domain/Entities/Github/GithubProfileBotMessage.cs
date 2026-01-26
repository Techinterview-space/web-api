using System;

namespace Domain.Entities.Github;

public class GithubProfileBotMessage : HasDatesBase
{
    public Guid Id { get; protected set; }

    public string Text { get; protected set; }

    public Guid GithubProfileBotChatId { get; protected set; }

    public virtual GithubProfileBotChat GithubProfileBotChat { get; protected set; }

    public GithubProfileBotMessage(
        string text,
        GithubProfileBotChat githubProfileBotChat)
    {
        Text = text?.Trim();
        GithubProfileBotChatId = githubProfileBotChat.Id;
        GithubProfileBotChat = githubProfileBotChat;
    }

    protected GithubProfileBotMessage()
    {
    }
}