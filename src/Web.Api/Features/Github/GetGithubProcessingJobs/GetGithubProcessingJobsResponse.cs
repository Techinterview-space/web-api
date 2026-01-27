using System.Collections.Generic;
using Web.Api.Features.Github.Dtos;

namespace Web.Api.Features.Github.GetGithubProcessingJobs;

#pragma warning disable SA1313
public record GetGithubProcessingJobsResponse(
    IReadOnlyCollection<GithubProfileProcessingJobDto> Results);