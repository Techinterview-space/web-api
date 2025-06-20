using System.Collections.Generic;
using Domain.ValueObjects.Pagination;
using Web.Api.Features.Github.Dtos;

namespace Web.Api.Features.Github.GetGithubProfiles;

public record GetGithubProfilesResponse(
    int CurrentPage,
    int PageSize,
    int TotalItems,
    IReadOnlyCollection<GithubProfileDto> Results) : Pageable<GithubProfileDto>(CurrentPage, PageSize, TotalItems, Results);