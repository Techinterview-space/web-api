using System.Collections.Generic;
using Web.Api.Features.Github.Dtos;

namespace Web.Api.Features.Github.GetGithubProcessingJobs;

public record GetGithubProcessingJobsResponse(
    IReadOnlyCollection<GithubProfileProcessingJobDto> Results);