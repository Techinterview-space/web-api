using System;
using Domain.Entities.Github;

namespace Web.Api.Features.Github.Dtos;

public record GithubProfileDto
{
    public string Username { get; init; }
    
    public int Version { get; init; }
    
    public int RequestsCount { get; init; }
    
    public DateTime DataSyncedAt { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public DateTimeOffset UpdatedAt { get; init; }

    public GithubProfileDto()
    {
    }

    public GithubProfileDto(GithubProfile profile)
    {
        Username = profile.Username;
        Version = profile.Version;
        RequestsCount = profile.RequestsCount;
        DataSyncedAt = profile.DataSyncedAt;
        CreatedAt = profile.CreatedAt;
        UpdatedAt = profile.UpdatedAt;
    }
}