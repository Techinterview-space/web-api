using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.Github.GetGithubProfileChats;

public record GetGithubProfileChatsQueryParams : PageModel
{
    public string Search { get; init; } = string.Empty;
}