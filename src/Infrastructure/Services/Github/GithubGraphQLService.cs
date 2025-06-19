using System.Text.Json;
using Domain.Entities.Github;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Github;

public class GithubGraphQlService : IGithubGraphQLService, IDisposable
{
    private const int DefaultBatchSize = 10;
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

    public async Task<GithubProfileData> GetUserProfileDataAsync(
        string username,
        int monthsToFetchCommits = 6,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync(cancellationToken);
            var since = DateTimeOffset.UtcNow.AddMonths(-monthsToFetchCommits).ToString("yyyy-MM-ddTHH:mm:ss");

            _logger.LogInformation(
                "Fetching GitHub profile data for user {Username} using GraphQL API",
                username);

            // Single GraphQL query to get user data and repositories first
            var userQuery = new GraphQLRequest
            {
                Query = @"
                query GetUserProfile($username: String!) {
                  user(login: $username) {
                    name
                    login
                    url
                    followers {
                      totalCount
                    }
                    following {
                      totalCount
                    }
                    publicRepos: repositories(first: 100, ownerAffiliations: OWNER, privacy: PUBLIC) {
                      totalCount
                    }
                    repositories(first: 100, ownerAffiliations: OWNER) {
                      totalCount
                      nodes {
                        name
                        owner {
                          login
                        }
                        stargazerCount
                        isFork
                        defaultBranchRef {
                          name
                        }
                      }
                    }
                    organizations(first: 50) {
                      nodes {
                        login
                        repositories(first: 100) {
                          nodes {
                            name
                            owner {
                              login
                            }
                            stargazerCount
                            isFork
                            defaultBranchRef {
                              name
                            }
                          }
                        }
                      }
                    }
                    issues {
                      totalCount
                    }
                    pullRequests {
                      totalCount
                    }
                    createdAt
                  }
                }",
                Variables = new
                {
                    username = username
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
                throw new InvalidOperationException($"User {username} not found");
            }

            _logger.LogInformation("Successfully fetched basic profile data for user {Username}", username);

            // Now get commit statistics efficiently using a batched approach
            var commitStats = await GetCommitStatisticsAsync(client, username, response.Data.User, since, cancellationToken);

            _logger.LogInformation(
                "Successfully fetched commit statistics for user {Username}: {CommitsCount} commits, {FilesAdjusted} files",
                username,
                commitStats.CommitsCount,
                commitStats.FilesAdjusted);

            return MapToGithubProfileData(response.Data.User, commitStats, monthsToFetchCommits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch GitHub profile data for user {Username}", username);
            throw;
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
        UserProfile user,
        string since,
        CancellationToken cancellationToken)
    {
        var allRepos = new List<Repository>();

        // Collect all repositories (user owned + organization)
        if (user.Repositories?.Nodes != null)
        {
            allRepos.AddRange(user.Repositories.Nodes);
        }

        if (user.Organizations?.Nodes != null)
        {
            foreach (var org in user.Organizations.Nodes)
            {
                if (org.Repositories?.Nodes != null)
                {
                    allRepos.AddRange(org.Repositories.Nodes);
                }
            }
        }

        // Process repositories in batches to get commit stats
        var stats = new CommitStatistics();

        _logger.LogInformation(
            "Processing {TotalRepos} repositories in batches of {BatchSize} for user {Username}",
            allRepos.Count,
            DefaultBatchSize,
            username);

        for (var i = 0; i < allRepos.Count; i += DefaultBatchSize)
        {
            var batch = allRepos.Skip(i).Take(DefaultBatchSize).ToList();
            var batchStats = await GetCommitStatisticsForBatch(client, username, batch, since, cancellationToken);

            stats.CommitsCount += batchStats.CommitsCount;
            stats.FilesAdjusted += batchStats.FilesAdjusted;
            stats.ChangesInFilesCount += batchStats.ChangesInFilesCount;
            stats.AdditionsInFilesCount += batchStats.AdditionsInFilesCount;
            stats.DeletionsInFilesCount += batchStats.DeletionsInFilesCount;

            _logger.LogDebug(
                "Processed batch {BatchNumber}/{TotalBatches} for user {Username}",
                (i / DefaultBatchSize) + 1,
                (allRepos.Count + DefaultBatchSize - 1) / DefaultBatchSize,
                username);
        }

        return stats;
    }

    private async Task<CommitStatistics> GetCommitStatisticsForBatch(
        GraphQLHttpClient client,
        string username,
        List<Repository> repositories,
        string since,
        CancellationToken cancellationToken)
    {
        if (repositories.Count == 0)
        {
            return new CommitStatistics();
        }

        // Build a dynamic GraphQL query for the batch
        var repoQueries = new List<string>();
        var variables = new Dictionary<string, object>
        {
            ["username"] = username,
            ["since"] = since
        };

        for (var i = 0; i < repositories.Count; i++)
        {
            var repo = repositories[i];
            var alias = $"repo{i}";
            variables[$"owner{i}"] = repo.Owner.Login;
            variables[$"name{i}"] = repo.Name;

            repoQueries.Add($@"
                {alias}: repository(owner: $owner{i}, name: $name{i}) {{
                  defaultBranchRef {{
                    target {{
                      ... on Commit {{
                        history(first: 100, since: $since) {{
                          totalCount
                          nodes {{
                            committedDate
                            additions
                            deletions
                            changedFiles
                          }}
                        }}
                      }}
                    }}
                  }}
                }}");
        }

        var batchQuery = new GraphQLRequest
        {
            Query = $@"
                query GetCommitsBatch($username: String!, $since: GitTimestamp!, {string.Join(", ", repositories.Select((r, i) => $"$owner{i}: String!, $name{i}: String!"))}) {{
                  {string.Join("\n", repoQueries)}
                }}",
            Variables = variables
        };

        try
        {
            var response = await client.SendQueryAsync<Dictionary<string, object>>(batchQuery, cancellationToken);

            if (response.Errors?.Any() == true)
            {
                _logger.LogWarning(
                    "GraphQL errors in batch: {Errors}",
                    string.Join(", ", response.Errors.Select(e => e.Message)));
            }

            return ProcessBatchCommitResponse(response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to get commit statistics for batch, skipping");

            return new CommitStatistics();
        }
    }

    private CommitStatistics ProcessBatchCommitResponse(
        Dictionary<string, object> data)
    {
        var stats = new CommitStatistics();

        if (data == null)
        {
            return stats;
        }

        try
        {
            // Convert dynamic data to JSON and parse it properly
            var json = JsonSerializer.Serialize(data);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            foreach (var property in jsonElement.EnumerateObject())
            {
                try
                {
                    var repoData = property.Value;

                    if (repoData.TryGetProperty("defaultBranchRef", out var branchRef) &&
                        branchRef.TryGetProperty("target", out var target) &&
                        target.TryGetProperty("history", out var history) &&
                        history.TryGetProperty("nodes", out var nodes))
                    {
                        foreach (var commit in nodes.EnumerateArray())
                        {
                            // For now, count all commits since filtering by author in GraphQL is complex
                            // In a future iteration; we could add better author filtering
                            stats.CommitsCount++;

                            if (commit.TryGetProperty("changedFiles", out var changedFiles))
                            {
                                stats.FilesAdjusted += changedFiles.GetInt32();
                            }

                            if (commit.TryGetProperty("additions", out var additions))
                            {
                                var additionsCount = additions.GetInt32();
                                stats.AdditionsInFilesCount += additionsCount;
                                stats.ChangesInFilesCount += additionsCount;
                            }

                            if (commit.TryGetProperty("deletions", out var deletions))
                            {
                                var deletionsCount = deletions.GetInt32();
                                stats.DeletionsInFilesCount += deletionsCount;
                                stats.ChangesInFilesCount += deletionsCount;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to process commit data for repository {RepoKey}",
                        property.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process batch commit response");
        }

        return stats;
    }

    private GithubProfileData MapToGithubProfileData(UserProfile user, CommitStatistics commitStats, int monthsToFetchCommits)
    {
        var totalStars = 0;
        var forkedReposCount = 0;

        // Process user repositories for stars and forks count
        if (user.Repositories?.Nodes != null)
        {
            foreach (var repo in user.Repositories.Nodes)
            {
                totalStars += repo.StargazerCount;
                if (repo.IsFork)
                {
                    forkedReposCount++;
                }
            }
        }

        return new GithubProfileData
        {
            Name = user.Name ?? user.Login,
            Username = user.Login,
            HtmlUrl = user.Url,
            Followers = user.Followers?.TotalCount ?? 0,
            Following = user.Following?.TotalCount ?? 0,
            PublicRepos = user.PublicRepos?.TotalCount ?? 0,
            TotalPrivateRepos = 0, // GraphQL doesn't expose private repo count for other users
            PullRequestsCreatedByUser = user.PullRequests?.TotalCount ?? 0,
            IssuesOpenedByUser = user.Issues?.TotalCount ?? 0,
            CountOfStarredRepos = totalStars,
            CountOfForkedRepos = forkedReposCount,
            CommitsCount = commitStats.CommitsCount,
            FilesAdjusted = commitStats.FilesAdjusted,
            ChangesInFilesCount = commitStats.ChangesInFilesCount,
            AdditionsInFilesCount = commitStats.AdditionsInFilesCount,
            DeletionsInFilesCount = commitStats.DeletionsInFilesCount,
            CreatedAt = user.CreatedAt
        };
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

    // Response DTOs for GraphQL
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

    public record UserProfile
    {
        public string Name { get; set; }

        public string Login { get; set; }

        public string Url { get; set; }

        public CountInfo Followers { get; set; }

        public CountInfo Following { get; set; }

        public CountInfo PublicRepos { get; set; }

        public RepositoryConnection Repositories { get; set; }

        public OrganizationConnection Organizations { get; set; }

        public CountInfo Issues { get; set; }

        public CountInfo PullRequests { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class CountInfo
    {
        public int TotalCount { get; set; }
    }

    public record RepositoryConnection
    {
        public int TotalCount { get; set; }

        public List<Repository> Nodes { get; set; }
    }

    public record OrganizationConnection
    {
        public List<Organization> Nodes { get; set; }
    }

    public record Organization
    {
        public string Login { get; set; }

        public RepositoryConnection Repositories { get; set; }
    }

    public class Repository
    {
        public string Name { get; set; }

        public Owner Owner { get; set; }

        public int StargazerCount { get; set; }

        public bool IsFork { get; set; }

        public DefaultBranchRef DefaultBranchRef { get; set; }
    }

    public record Owner
    {
        public string Login { get; set; }
    }

    public record DefaultBranchRef
    {
        public string Name { get; set; }
    }
}