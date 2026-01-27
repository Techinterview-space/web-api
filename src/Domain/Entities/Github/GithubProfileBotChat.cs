using System;
using System.Collections.Generic;

namespace Domain.Entities.Github;

public class GithubProfileBotChat : HasDatesBase
{
    public Guid Id { get; protected set; }

    public long ChatId { get; protected set; }

    public string Username { get; protected set; }

    public bool IsAdmin { get; protected set; }

    public int MessagesCount { get; protected set; }

    public virtual List<GithubProfileBotMessage> Messages { get; protected set; }

    public GithubProfileBotChat(
        long chatId,
        string username,
        bool isAdmin = false)
    {
        ChatId = chatId;
        Username = username?.Trim();
        IsAdmin = isAdmin;
        MessagesCount = 1;
        Messages = new List<GithubProfileBotMessage>();
    }

    public void IncrementMessagesCount()
    {
        MessagesCount++;
    }

    protected GithubProfileBotChat()
    {
    }
}