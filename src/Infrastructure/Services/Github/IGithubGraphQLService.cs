namespace Infrastructure.Services.Github;

public interface IGithubGraphQLService
{
    Task<GithubProfileDataResult> GetUserProfileDataAsync(
        string username,
        int monthsToFetchCommits = 6,
        CancellationToken cancellationToken = default);
}