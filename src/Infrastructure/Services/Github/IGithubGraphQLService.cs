using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Github;

namespace Infrastructure.Services.Github;

public interface IGithubGraphQLService
{
    Task<GithubProfileData> GetUserProfileDataAsync(
        string username,
        int monthsToFetchCommits = 6,
        CancellationToken cancellationToken = default);
}