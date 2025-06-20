using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    public async Task<SearchIssuesResult> SearchUserDiscussionsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        // Note: GitHub REST API doesn't have direct discussion search capability.
        // This is a placeholder that returns zero results as discussions are better 
        // handled through GraphQL API. For REST API fallback, we return empty result.
        return new SearchIssuesResult(0, false, new List<Issue>());
    }

    public async Task<int> GetUserCodeReviewsCountAsync(
        string username,
        int monthsToFetch = 6,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Search for pull requests reviewed by the user
            var reviewedPRs = await (await GetClientAsync(cancellationToken)).Search.SearchIssues(
                new SearchIssuesRequest
                {
                    Type = IssueTypeQualifier.PullRequest,
                    State = ItemState.All,
                    Reviewed = username,
                    Updated = DateRange.GreaterThan(DateTimeOffset.UtcNow.AddMonths(-monthsToFetch)),
                });

            return reviewedPRs.TotalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to fetch code reviews count for user {Username}",
                username);

            return 0;
        }
    }

    public async Task<string> GetTopLanguagesByCommitsAsync(
        IReadOnlyList<Repository> repositories,
        string username,
        int monthsToFetch = 6,
        CancellationToken cancellationToken = default)
    {
        var languageCommitCounts = new Dictionary<string, int>();

        foreach (var repo in repositories.Take(10)) // Limit to avoid too many API calls
        {
            try
            {
                if (repo.Language != null)
                {
                    var commits = await GetRepositoryCommitsAsync(
                        repo.Owner.Login,
                        repo.Name,
                        username,
                        monthsToFetch,
                        cancellationToken);

                    if (commits.Count > 0)
                    {
                        languageCommitCounts[repo.Language] = 
                            languageCommitCounts.GetValueOrDefault(repo.Language, 0) + commits.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to fetch commits for repository {RepoOwner}/{RepoName}",
                    repo.Owner.Login,
                    repo.Name);
            }
        }

        // Get top 3 languages
        var topLanguages = languageCommitCounts
            .OrderByDescending(x => x.Value)
            .Take(3)
            .Select(x => $"{x.Key} ({x.Value})")
            .ToArray();

        return topLanguages.Length > 0 ? string.Join(", ", topLanguages) : string.Empty;
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