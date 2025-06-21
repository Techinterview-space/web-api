using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.Github.GetGithubProfiles;

public record GetGithubProfilesQueryParams : PageModel
{
    public string Search { get; init; } = string.Empty;
}