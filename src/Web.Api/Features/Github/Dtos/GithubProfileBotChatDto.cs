using System;
using Domain.Entities.Github;

namespace Web.Api.Features.Github.Dtos;

public record GithubProfileBotChatDto
{
    public Guid Id { get; init; }
    
    public long ChatId { get; init; }
    
    public string Username { get; init; }
    
    public bool IsAdmin { get; init; }
    
    public int MessagesCount { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public DateTimeOffset UpdatedAt { get; init; }

    public GithubProfileBotChatDto()
    {
    }

    public GithubProfileBotChatDto(GithubProfileBotChat chat)
    {
        Id = chat.Id;
        ChatId = chat.ChatId;
        Username = chat.Username;
        IsAdmin = chat.IsAdmin;
        MessagesCount = chat.MessagesCount;
        CreatedAt = chat.CreatedAt;
        UpdatedAt = chat.UpdatedAt;
    }
}