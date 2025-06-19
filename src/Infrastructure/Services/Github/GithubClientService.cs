using Microsoft.Extensions.Logging;
using Octokit;
using Octokit.Internal;

namespace Infrastructure.Services.Github;

// TODO mgorbatyuk: create interface for this service
public class GithubClientService
{
    private readonly IGithubPersonalUserTokenService _githubPersonalUserTokenService;
    private readonly ILogger<GithubClientService> _logger;

    private GitHubClient _client;

    public GithubClientService(
        IGithubPersonalUserTokenService githubPersonalUserTokenService,
        ILogger<GithubClientService> logger)
    {
        _githubPersonalUserTokenService = githubPersonalUserTokenService;
        _logger = logger;
    }

    public async Task<User> GetUserAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return await (await GetClientAsync(cancellationToken)).User.Get(username);
    }

    public async Task<IReadOnlyList<Organization>> GetOrganizationsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return await (await GetClientAsync(cancellationToken)).Organization.GetAllForUser(username);
    }

    public async Task<IReadOnlyList<Repository>> GetUserRepositoriesAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return await (await GetClientAsync(cancellationToken)).Repository.GetAllForUser(username);
    }

    public async Task<IReadOnlyList<Repository>> GetOrganizationRepositoriesAsync(
        string organizationLogin,
        CancellationToken cancellationToken = default)
    {
        return await (await GetClientAsync(cancellationToken)).Repository
            .GetAllForOrg(organizationLogin);
    }

    public async Task<IReadOnlyList<GitHubCommit>> GetRepositoryCommitsAsync(
        string repoOwner,
        string repositoryName,
        string username,
        int monthsToFetchCommits = 6,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await (await GetClientAsync(cancellationToken)).Repository.Commit.GetAll(
                repoOwner,
                repositoryName,
                new CommitRequest
                {
                    Author = username,
                    Since = DateTimeOffset.UtcNow.AddMonths(-monthsToFetchCommits),
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to fetch commits for repository {RepoOwner}/{RepositoryName} by user {Username}",
                repoOwner,
                repositoryName,
                username);

            return new List<GitHubCommit>();
        }
    }

    public async Task<GitHubCommit> GetCommitAsync(
        string repoOwner,
        string repositoryName,
        string commitSha,
        CancellationToken cancellationToken = default)
    {
        return await (await GetClientAsync(cancellationToken)).Repository.Commit.Get(
            repoOwner,
            repositoryName,
            commitSha);
    }

    public async Task<SearchIssuesResult> SearchUserIssuesAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return await (await GetClientAsync(cancellationToken)).Search.SearchIssues(
            new SearchIssuesRequest
            {
                Author = username,
                Type = IssueTypeQualifier.Issue,
            });
    }

    public async Task<SearchIssuesResult> SearchUserPullRequestsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return await (await GetClientAsync(cancellationToken)).Search.SearchIssues(
            new SearchIssuesRequest
            {
                Author = username,
                Type = IssueTypeQualifier.PullRequest,
            });
    }

    private async Task<GitHubClient> GetClientAsync(
        CancellationToken cancellationToken = default)
    {
        if (_client != null)
        {
            return _client;
        }

        var patToken = await _githubPersonalUserTokenService.GetTokenAsync(cancellationToken);
        _client = new GitHubClient(
            new ProductHeaderValue("techinterview.space"),
            new InMemoryCredentialStore(
                new Credentials(patToken)));

        return _client;
    }
}