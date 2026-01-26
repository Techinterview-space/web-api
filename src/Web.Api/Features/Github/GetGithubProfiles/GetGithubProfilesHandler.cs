using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Github.Dtos;

namespace Web.Api.Features.Github.GetGithubProfiles;

public class GetGithubProfilesHandler : IRequestHandler<GetGithubProfilesQueryParams, GetGithubProfilesResponse>
{
    private readonly DatabaseContext _context;

    public GetGithubProfilesHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<GetGithubProfilesResponse> Handle(
        GetGithubProfilesQueryParams request,
        CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim().ToLowerInvariant();
        var profiles = await _context.GithubProfiles
            .When(
                !string.IsNullOrEmpty(search),
                x => x.Username.ToLower().Contains(search))
            .OrderByDescending(x => x.CreatedAt)
            .AsPaginatedAsync(
                request,
                cancellationToken);

        return new GetGithubProfilesResponse(
            profiles.CurrentPage,
            profiles.PageSize,
            profiles.TotalItems,
            profiles.Results
                .Select(x => new GithubProfileDto(x))
                .ToList());
    }
}