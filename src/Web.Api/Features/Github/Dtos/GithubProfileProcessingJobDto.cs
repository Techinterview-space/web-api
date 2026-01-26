using System;
using Domain.Entities.Github;

namespace Web.Api.Features.Github.Dtos;

public record GithubProfileProcessingJobDto
{
    public string Username { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public GithubProfileProcessingJobDto()
    {
    }

    public GithubProfileProcessingJobDto(GithubProfileProcessingJob job)
    {
        Username = job.Username;
        CreatedAt = job.CreatedAt;
        UpdatedAt = job.UpdatedAt;
    }
}