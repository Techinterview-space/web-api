using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Github;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Github;

public class GithubGraphQlService : IGithubGraphQLService, IDisposable
{
    private readonly IGithubPersonalUserTokenService _githubPersonalUserTokenService;
    private readonly ILogger<GithubGraphQlService> _logger;

    private GraphQLHttpClient _client;

    public GithubGraphQlService(
        IGithubPersonalUserTokenService githubPersonalUserTokenService,
        ILogger<GithubGraphQlService> logger)
    {
        _githubPersonalUserTokenService = githubPersonalUserTokenService;
        _logger = logger;
    }

    public async Task<GithubProfileDataResult> GetUserProfileDataAsync(
        string username,
        int monthsToFetchCommits = 6,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync(cancellationToken);
            var since = DateTimeOffset.UtcNow.AddMonths(-monthsToFetchCommits).ToString("yyyy-MM-ddTHH:mm:ss");
            var until = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

            _logger.LogInformation(
                "Fetching GitHub profile data for user {Username} using GraphQL API",
                username);

            // Step 1: Get user ID and repository contributions (similar to Python implementation)
            var userQuery = new GraphQLRequest
            {
                Query = @"
                query GetUserProfile($username: String!, $from: DateTime!, $to: DateTime!) {
                  user(login: $username) {
                    id
                    name
                    login
                    url
                    followers {
                      totalCount
                    }
                    following {
                      totalCount
                    }
                    // Include both user-owned repositories and organization repositories 
                    // to ensure all contribution statistics are captured correctly
                    repositories(first: 100, ownerAffiliations: [OWNER, ORGANIZATION_MEMBER]) {
                      totalCount
                      nodes {
                        stargazerCount
                        forkCount
                        isFork
                      }
                    }
                    starredRepositories {
                      totalCount
                    }
                    contributionsCollection(from: $from, to: $to) {
                      totalPullRequestReviewContributions
                      commitContributionsByRepository {
                        repository {
                          name
                          owner { 
                            login 
                          }
                          primaryLanguage {
                            name
                          }
                        }
                        contributions {
                          totalCount
                        }
                      }
                    }
                    issues(first: 100, states: [OPEN, CLOSED]) {
                      totalCount
                    }
                    pullRequests(first: 100, states: [OPEN, MERGED, CLOSED]) {
                      totalCount
                    }
                    repositoryDiscussions {
                      totalCount
                    }
                    createdAt
                  }
                }",
                Variables = new
                {
                    username = username,
                    from = DateTimeOffset.UtcNow.AddMonths(-monthsToFetchCommits),
                    to = DateTimeOffset.UtcNow
                }
            };

            var response = await client.SendQueryAsync<UserProfileResponse>(userQuery, cancellationToken);

            if (response.Errors?.Length > 0)
            {
                _logger.LogWarning(
                    "GraphQL errors for user {Username}: {Errors}",
                    username,
                    string.Join(", ", response.Errors.Select(e => e.Message)));
            }

            if (response.Data?.User == null)
            {
                return GithubProfileDataResult.NotFound();
            }

            _logger.LogInformation(
                "Successfully fetched basic profile data for user {Username}",
                username);

            // Step 2: Get detailed commit statistics using user ID filtering (Python pattern)
            var commitStats = await GetCommitStatisticsAsync(
                client,
                username,
                response.Data.User.Id,
                response.Data.User.ContributionsCollection?.CommitContributionsByRepository ?? new List<RepositoryContribution>(),
                since,
                until,
                cancellationToken);

            _logger.LogInformation(
                "Successfully fetched commit statistics for user {Username}: {CommitsCount} commits, {FilesAdjusted} files",
                username,
                commitStats.CommitsCount,
                commitStats.FilesAdjusted);

            return GithubProfileDataResult.Success(
                MapToGithubProfileData(
                    user: response.Data.User,
                    commitStats: commitStats,
                    monthsToFetchCommits: monthsToFetchCommits,
                    topLanguagesCount: 3));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to fetch GitHub profile data for user {Username}",
                username);

            return GithubProfileDataResult.Failure(
                $"Error fetching GitHub profile data for {username}");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(
        bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();
            _client = null;
        }
    }

    private async Task<CommitStatistics> GetCommitStatisticsAsync(
        GraphQLHttpClient client,
        string username,
        string userId,
        List<RepositoryContribution> repositoryContributions,
        string since,
        string until,
        CancellationToken cancellationToken)
    {
        var stats = new CommitStatistics();

        _logger.LogInformation(
            "Fetching detailed commit data for user {Username} (ID: {UserId}) from {RepositoryCount} repositories",
            username,
            userId,
            repositoryContributions.Count);

        // Log the repositories being processed for better debugging
        foreach (var repo in repositoryContributions)
        {
            _logger.LogDebug(
                "Processing repository: {Owner}/{Name} (contributions: {ContributionCount})",
                repo.Repository.Owner.Login,
                repo.Repository.Name,
                repo.Contributions?.TotalCount ?? 0);
        }

        // Process each repository to get detailed commit information (following Python pattern)
        foreach (var repoContribution in repositoryContributions)
        {
            try
            {
                var repoStats = await GetCommitsFromRepositoryAsync(
                    client,
                    repoContribution.Repository.Owner.Login,
                    repoContribution.Repository.Name,
                    userId,
                    since,
                    until,
                    cancellationToken);

                stats.CommitsCount += repoStats.CommitsCount;
                stats.FilesAdjusted += repoStats.FilesAdjusted;
                stats.AdditionsInFilesCount += repoStats.AdditionsInFilesCount;
                stats.DeletionsInFilesCount += repoStats.DeletionsInFilesCount;
                stats.ChangesInFilesCount += repoStats.ChangesInFilesCount;

                _logger.LogDebug(
                    "Repository {Owner}/{Name}: {Commits} commits, {Files} files, {Additions} additions, {Deletions} deletions (Owner type: {OwnerType})",
                    repoContribution.Repository.Owner.Login,
                    repoContribution.Repository.Name,
                    repoStats.CommitsCount,
                    repoStats.FilesAdjusted,
                    repoStats.AdditionsInFilesCount,
                    repoStats.DeletionsInFilesCount,
                    repoContribution.Repository.Owner.Login == username ? "User" : "Organization");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to get commits from repository {Owner}/{Name}",
                    repoContribution.Repository.Owner.Login,
                    repoContribution.Repository.Name);
            }
        }

        var userRepoCount = repositoryContributions.Count(r => r.Repository.Owner.Login == username);
        var orgRepoCount = repositoryContributions.Count(r => r.Repository.Owner.Login != username);

        _logger.LogInformation(
            "Completed commit statistics for user {Username}: {TotalCommits} commits from {UserRepoCount} user repos and {OrgRepoCount} organization repos",
            username,
            stats.CommitsCount,
            userRepoCount,
            orgRepoCount);

        return stats;
    }

    private async Task<CommitStatistics> GetCommitsFromRepositoryAsync(
        GraphQLHttpClient client,
        string owner,
        string repo,
        string userId,
        string since,
        string until,
        CancellationToken cancellationToken)
    {
        var stats = new CommitStatistics();
        string cursor = null;

        var repoCommitsQuery = @"
        query($owner: String!, $repo: String!, $user_id: ID!, $since: GitTimestamp!, $until: GitTimestamp!, $cursor: String) {
          repository(owner: $owner, name: $repo) {
            object(expression: ""HEAD"") {
              ... on Commit {
                history(first: 100, since: $since, until: $until, author: {id: $user_id}, after: $cursor) {
                  pageInfo {
                    hasNextPage
                    endCursor
                  }
                  nodes {
                    oid
                    committedDate
                    additions
                    deletions
                    changedFiles
                    author {
                      user {
                        login
                      }
                    }
                  }
                }
              }
            }
          }
        }";

        try
        {
            do
            {
                var query = new GraphQLRequest
                {
                    Query = repoCommitsQuery,
                    Variables = new
                    {
                        owner = owner,
                        repo = repo,
                        user_id = userId,
                        since = since,
                        until = until,
                        cursor = cursor
                    }
                };

                var response = await client.SendQueryAsync<RepositoryCommitResponse>(query, cancellationToken);

                if (response.Errors?.Any() == true)
                {
                    _logger.LogWarning(
                        "GraphQL errors fetching commits from {Owner}/{Repo}: {Errors}",
                        owner,
                        repo,
                        string.Join(", ", response.Errors.Select(e => e.Message)));
                }

                // Check if repository or object exists (similar to Python error handling)
                if (response.Data?.Repository?.Object?.History == null)
                {
                    _logger.LogDebug("Repository {Owner}/{Repo} has no commit history or is inaccessible", owner, repo);
                    break;
                }

                var history = response.Data.Repository.Object.History;
                var commits = history.Nodes ?? new List<CommitNode>();

                _logger.LogDebug("Found {CommitCount} commits in {Owner}/{Repo}", commits.Count, owner, repo);

                // Process commits (they are already filtered by user ID in the query)
                foreach (var commit in commits)
                {
                    stats.CommitsCount++;
                    stats.FilesAdjusted += commit.ChangedFiles;
                    stats.AdditionsInFilesCount += commit.Additions;
                    stats.DeletionsInFilesCount += commit.Deletions;
                    stats.ChangesInFilesCount += commit.Additions + commit.Deletions;
                }

                // Handle pagination (following Python pattern)
                if (!history.PageInfo.HasNextPage)
                {
                    break;
                }

                cursor = history.PageInfo.EndCursor;
            }
            while (true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Error fetching commits from {Owner}/{Repo}: {Message}",
                owner,
                repo,
                ex.Message);
        }

        return stats;
    }

    private GithubProfileData MapToGithubProfileData(
        UserProfile user,
        CommitStatistics commitStats,
        int monthsToFetchCommits,
        int topLanguagesCount)
    {
        return new GithubProfileData
        {
            Name = user.Name ?? user.Login,
            Username = user.Login,
            HtmlUrl = user.Url,
            Followers = user.Followers?.TotalCount ?? 0,
            Following = user.Following?.TotalCount ?? 0,
            PublicRepos = user.Repositories?.TotalCount ?? 0,
            TotalPrivateRepos = 0, // GraphQL doesn't expose private repo count for other users
            PullRequestsCreatedByUser = user.PullRequests?.TotalCount ?? 0,
            IssuesOpenedByUser = user.Issues?.TotalCount ?? 0,
            CountOfStarredRepos = user.StarredRepositories?.TotalCount ?? 0,
            CountOfForkedRepos = user.Repositories?.Nodes?.Count(r => r.IsFork) ?? 0,
            CommitsCount = commitStats.CommitsCount,
            FilesAdjusted = commitStats.FilesAdjusted,
            ChangesInFilesCount = commitStats.ChangesInFilesCount,
            AdditionsInFilesCount = commitStats.AdditionsInFilesCount,
            DeletionsInFilesCount = commitStats.DeletionsInFilesCount,
            DiscussionsOpened = user.RepositoryDiscussions?.TotalCount ?? 0,
            CodeReviewsMade = user.ContributionsCollection?.TotalPullRequestReviewContributions ?? 0,
            TopLanguagesByCommits = CalculateTopLanguages(
                user.ContributionsCollection?.CommitContributionsByRepository,
                topLanguagesCount),
            MonthsToFetchCommits = monthsToFetchCommits,
            UserCreatedAt = user.CreatedAt,
            CreatedAt = DateTime.UtcNow,
        };
    }

    private Dictionary<string, int> CalculateTopLanguages(
        List<RepositoryContribution> repositoryContributions,
        int topLanguagesCount = 3)
    {
        if (repositoryContributions == null || repositoryContributions.Count == 0)
        {
            return new Dictionary<string, int>();
        }

        var languageCommitCounts = new Dictionary<string, int>();

        foreach (var repoContrib in repositoryContributions)
        {
            if (repoContrib.Repository?.PrimaryLanguage?.Name != null)
            {
                var language = repoContrib.Repository.PrimaryLanguage.Name;
                var commitCount = repoContrib.Contributions?.TotalCount ?? 0;
                languageCommitCounts[language] = languageCommitCounts.GetValueOrDefault(language, 0) + commitCount;
            }
        }

        // Get top 3 languages
        return new Dictionary<string, int>(
            languageCommitCounts
                .OrderByDescending(x => x.Value)
                .Take(topLanguagesCount)
                .ToList());
    }

    private async Task<GraphQLHttpClient> GetClientAsync(
        CancellationToken cancellationToken = default)
    {
        if (_client != null)
        {
            return _client;
        }

        var token = await _githubPersonalUserTokenService.GetTokenAsync(cancellationToken);

        _client = new GraphQLHttpClient("https://api.github.com/graphql", new NewtonsoftJsonSerializer());
        _client.HttpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
        _client.HttpClient.DefaultRequestHeaders.Add("User-Agent", "techinterview.space");

        return _client;
    }

    // Response DTOs for GraphQL (simplified following Python pattern)
    public record CommitStatistics
    {
        public int CommitsCount { get; set; }

        public int FilesAdjusted { get; set; }

        public int ChangesInFilesCount { get; set; }

        public int AdditionsInFilesCount { get; set; }

        public int DeletionsInFilesCount { get; set; }
    }

    public record UserProfileResponse
    {
        public UserProfile User { get; set; }
    }

    public record RepositoryCommitResponse
    {
        public RepositoryData Repository { get; set; }
    }

    public record RepositoryData
    {
        public CommitObject Object { get; set; }
    }

    public record CommitObject
    {
        public CommitHistory History { get; set; }
    }

    public record CommitHistory
    {
        public PageInfo PageInfo { get; set; }
        public List<CommitNode> Nodes { get; set; }
    }

    public record PageInfo
    {
        public bool HasNextPage { get; set; }
        public string EndCursor { get; set; }
    }

    public record CommitNode
    {
        public string Oid { get; set; }
        public DateTime CommittedDate { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }
        public int ChangedFiles { get; set; }
        public CommitAuthor Author { get; set; }
    }

    public record CommitAuthor
    {
        public CommitUser User { get; set; }
    }

    public record CommitUser
    {
        public string Login { get; set; }
    }

    public record UserProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }

        public string Url { get; set; }

        public CountInfo Followers { get; set; }

        public CountInfo Following { get; set; }

        public RepositoriesInfo Repositories { get; set; }

        public CountInfo StarredRepositories { get; set; }
        public CountInfo Issues { get; set; }
        public CountInfo PullRequests { get; set; }
        public CountInfo RepositoryDiscussions { get; set; }
        public ContributionsCollection ContributionsCollection { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public record CountInfo
    {
        public int TotalCount { get; set; }
    }

    public record RepositoriesInfo
    {
        public int TotalCount { get; set; }
        public List<RepositoryNode> Nodes { get; set; }
    }

    public record RepositoryNode
    {
        public int StargazerCount { get; set; }
        public int ForkCount { get; set; }
        public bool IsFork { get; set; }
    }

    public record ContributionsCollection
    {
        public int TotalPullRequestReviewContributions { get; set; }
        public List<RepositoryContribution> CommitContributionsByRepository { get; set; }
    }

    public record RepositoryContribution
    {
        public ContributionRepository Repository { get; set; }
        public CountInfo Contributions { get; set; }
    }

    public record ContributionRepository
    {
        public string Name { get; set; }
        public Owner Owner { get; set; }
        public PrimaryLanguage PrimaryLanguage { get; set; }
    }

    public record PrimaryLanguage
    {
        public string Name { get; set; }
    }

    public record Owner
    {
        public string Login { get; set; }
    }
}