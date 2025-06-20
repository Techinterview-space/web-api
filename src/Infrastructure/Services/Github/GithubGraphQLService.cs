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
            var since = DateTimeOffset.UtcNow.AddMonths(-monthsToFetchCommits).ToString("yyyy-MM-dd");

            _logger.LogInformation(
                "Fetching GitHub profile data for user {Username} using GraphQL API",
                username);

            // Single GraphQL query to get user data and repositories first
            var userQuery = new GraphQLRequest
            {
                Query = @"
                query GetUserProfile($username: String!) {
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
            var commitStats = await GetCommitStatisticsAsync(
                client,
                username,
                response.Data.User,
                since,
                monthsToFetchCommits,
                cancellationToken);

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
        int monthsToFetchCommits,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the user's commit activity using contribution information
            // This is a more reliable approach than search
            var contributionQuery = new GraphQLRequest
            {
                Query = @"
                query GetUserContributions($username: String!, $from: DateTime!, $to: DateTime!) {
                  user(login: $username) {
                    contributionsCollection(from: $from, to: $to) {
                      totalCommitContributions
                      commitContributionsByRepository {
                        repository {
                          owner {
                            login
                          }
                          name
                        }
                        contributions(first: 100) {
                          nodes {
                            commitCount
                            occurredAt
                          }
                        }
                      }
                    }
                  }
                }",
                Variables = new
                {
                    username = username,
                    from = DateTimeOffset.UtcNow.AddMonths(-monthsToFetchCommits),
                    to = DateTimeOffset.UtcNow
                }
            };

            _logger.LogInformation(
                "Fetching contribution data for user {Username} from {From} to {To}",
                username,
                DateTimeOffset.UtcNow.AddMonths(-monthsToFetchCommits),
                DateTimeOffset.UtcNow);

            var response = await client.SendQueryAsync<ContributionResponse>(contributionQuery, cancellationToken);

            if (response.Errors?.Any() == true)
            {
                _logger.LogWarning(
                    "GraphQL errors in contribution query: {Errors}",
                    string.Join(", ", response.Errors.Select(e => e.Message)));
            }

            if (response.Data?.User?.ContributionsCollection == null)
            {
                _logger.LogWarning("No contribution data returned for user {Username}", username);
                return await GetCommitStatisticsUsingBatchedApproach(client, username, user, since, cancellationToken);
            }

            var contributionData = response.Data.User.ContributionsCollection;
            var stats = new CommitStatistics
            {
                CommitsCount = contributionData.TotalCommitContributions
            };

            _logger.LogInformation(
                "Found {TotalCommits} total commit contributions for user {Username}",
                stats.CommitsCount,
                username);

            // Get detailed commit information from repositories to calculate file changes
            if (contributionData.CommitContributionsByRepository != null)
            {
                await EnrichStatsWithFileChanges(client, username, contributionData.CommitContributionsByRepository, stats, since, cancellationToken);
            }

            _logger.LogInformation(
                "Successfully processed contribution data for user {Username}: {CommitsCount} commits, {FilesAdjusted} files",
                username,
                stats.CommitsCount,
                stats.FilesAdjusted);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get commit statistics using contribution API for user {Username}", username);

            // Fallback to the original batched approach if contribution API fails
            return await GetCommitStatisticsUsingBatchedApproach(client, username, user, since, cancellationToken);
        }
    }

    private async Task EnrichStatsWithFileChanges(
        GraphQLHttpClient client,
        string username,
        List<RepositoryContribution> repositoryContributions,
        CommitStatistics stats,
        string since,
        CancellationToken cancellationToken)
    {
        // For a subset of repositories with the most contributions, get detailed commit information
        var topRepos = repositoryContributions
            .OrderByDescending(rc => rc.Contributions?.Nodes?.Sum(n => n.CommitCount) ?? 0)
            .Take(10) // Limit to top 10 repositories to avoid rate limits
            .ToList();

        foreach (var repoContribution in topRepos)
        {
            try
            {
                var repoQuery = new GraphQLRequest
                {
                    Query = @"
                    query GetRepositoryCommitDetails($owner: String!, $name: String!, $since: GitTimestamp!, $first: Int!) {
                      repository(owner: $owner, name: $name) {
                        defaultBranchRef {
                          target {
                            ... on Commit {
                              history(first: $first, since: $since) {
                                nodes {
                                  author {
                                    user {
                                      login
                                    }
                                  }
                                  additions
                                  deletions
                                  changedFiles
                                }
                              }
                            }
                          }
                        }
                      }
                    }",
                    Variables = new
                    {
                        owner = repoContribution.Repository.Owner.Login,
                        name = repoContribution.Repository.Name,
                        since = since,
                        first = 100
                    }
                };

                var response = await client.SendQueryAsync<RepositoryCommitResponse>(repoQuery, cancellationToken);

                if (response.Data?.Repository?.DefaultBranchRef?.Target?.History?.Nodes != null)
                {
                    foreach (var commit in response.Data.Repository.DefaultBranchRef.Target.History.Nodes)
                    {
                        // Only count commits by the target user
                        if (commit.Author?.User?.Login?.Equals(username, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            stats.FilesAdjusted += commit.ChangedFiles;
                            stats.AdditionsInFilesCount += commit.Additions;
                            stats.DeletionsInFilesCount += commit.Deletions;
                            stats.ChangesInFilesCount += commit.Additions + commit.Deletions;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to get detailed commit data for repository {Owner}/{Name}",
                    repoContribution.Repository.Owner.Login,
                    repoContribution.Repository.Name);
            }
        }
    }

    private async Task<CommitStatistics> GetCommitStatisticsUsingBatchedApproach(
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
                  ... on Repository {{
                    defaultBranchRef {{
                      target {{
                        ... on Commit {{
                          history(first: 100, since: $since) {{
                            totalCount
                            nodes {{
                              author {{
                                user {{
                                  login
                                }}
                              }}
                              committedDate
                              additions
                              deletions
                              changedFiles
                            }}
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
                query GetCommitsBatch($since: GitTimestamp!, {string.Join(", ", repositories.Select((r, i) => $"$owner{i}: String!, $name{i}: String!"))}) {{
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

            if (response.Data == null)
            {
                _logger.LogWarning("No data returned from GraphQL batch query");
                return new CommitStatistics();
            }

            return ProcessBatchCommitResponse(response.Data, username);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogWarning(
                jsonEx,
                "JSON parsing error in batch commit query: {ErrorMessage}",
                jsonEx.Message);
            return new CommitStatistics();
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
        Dictionary<string, object> data,
        string username)
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

            _logger.LogDebug("Processing batch commit response for user {Username}", username);

            foreach (var property in jsonElement.EnumerateObject())
            {
                try
                {
                    var repoData = property.Value;

                    _logger.LogDebug("Processing repository data for {RepoKey}", property.Name);

                    // Check if repository exists (GraphQL returns null for non-existent repos)
                    if (repoData.ValueKind == JsonValueKind.Null)
                    {
                        _logger.LogDebug("Repository {RepoKey} not found or inaccessible", property.Name);
                        continue;
                    }

                    // Check if repository data exists and has the expected structure
                    if (!repoData.TryGetProperty("defaultBranchRef", out var branchRef) ||
                        branchRef.ValueKind == JsonValueKind.Null)
                    {
                        _logger.LogDebug("Repository {RepoKey} has no default branch", property.Name);
                        continue; // Repository has no default branch
                    }

                    if (!branchRef.TryGetProperty("target", out var target) ||
                        target.ValueKind == JsonValueKind.Null)
                    {
                        _logger.LogDebug("Repository {RepoKey} has no target (empty repository)", property.Name);
                        continue; // Target is null (empty repository)
                    }

                    if (!target.TryGetProperty("history", out var history) ||
                        history.ValueKind == JsonValueKind.Null)
                    {
                        _logger.LogDebug("Repository {RepoKey} has no commit history", property.Name);
                        continue; // No commit history
                    }

                    if (!history.TryGetProperty("nodes", out var nodes) ||
                        nodes.ValueKind == JsonValueKind.Null ||
                        nodes.ValueKind != JsonValueKind.Array)
                    {
                        _logger.LogDebug(
                            "Repository {RepoKey} has invalid nodes structure. ValueKind: {ValueKind}",
                            property.Name,
                            nodes.ValueKind);
                        continue; // No commit nodes or nodes is not an array
                    }

                    var commitsProcessed = 0;
                    foreach (var commit in nodes.EnumerateArray())
                    {
                        // Filter commits by author username
                        string authorLogin = null;
                        if (commit.TryGetProperty("author", out var author) &&
                            author.ValueKind != JsonValueKind.Null &&
                            author.TryGetProperty("user", out var user) &&
                            user.ValueKind != JsonValueKind.Null &&
                            user.TryGetProperty("login", out var login))
                        {
                            authorLogin = login.GetString();
                        }

                        // Only count commits authored by the specified username
                        if (!string.Equals(authorLogin, username, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        stats.CommitsCount++;
                        commitsProcessed++;

                        if (commit.TryGetProperty("changedFiles", out var changedFiles) &&
                            changedFiles.ValueKind == JsonValueKind.Number)
                        {
                            stats.FilesAdjusted += changedFiles.GetInt32();
                        }

                        if (commit.TryGetProperty("additions", out var additions) &&
                            additions.ValueKind == JsonValueKind.Number)
                        {
                            var additionsCount = additions.GetInt32();
                            stats.AdditionsInFilesCount += additionsCount;
                            stats.ChangesInFilesCount += additionsCount;
                        }

                        if (commit.TryGetProperty("deletions", out var deletions) &&
                            deletions.ValueKind == JsonValueKind.Number)
                        {
                            var deletionsCount = deletions.GetInt32();
                            stats.DeletionsInFilesCount += deletionsCount;
                            stats.ChangesInFilesCount += deletionsCount;
                        }
                    }

                    _logger.LogDebug(
                        "Processed {CommitsCount} commits for repository {RepoKey}",
                        commitsProcessed,
                        property.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to process commit data for repository {RepoKey}. Error: {ErrorMessage}",
                        property.Name,
                        ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process batch commit response. Error: {ErrorMessage}", ex.Message);
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

    public record ContributionResponse
    {
        public ContributionUser User { get; set; }
    }

    public record ContributionUser
    {
        public ContributionsCollection ContributionsCollection { get; set; }
    }

    public record ContributionsCollection
    {
        public int TotalCommitContributions { get; set; }
        public List<RepositoryContribution> CommitContributionsByRepository { get; set; }
    }

    public record RepositoryContribution
    {
        public ContributionRepository Repository { get; set; }
        public ContributionConnection Contributions { get; set; }
    }

    public record ContributionRepository
    {
        public Owner Owner { get; set; }
        public string Name { get; set; }
    }

    public record ContributionConnection
    {
        public List<ContributionNode> Nodes { get; set; }
    }

    public record ContributionNode
    {
        public int CommitCount { get; set; }
        public DateTime OccurredAt { get; set; }
    }

    public record RepositoryCommitResponse
    {
        public RepositoryCommitData Repository { get; set; }
    }

    public record RepositoryCommitData
    {
        public DefaultBranchRef DefaultBranchRef { get; set; }
    }

    public record CommitTarget
    {
        public CommitHistory History { get; set; }
    }

    public record CommitHistory
    {
        public List<CommitNode> Nodes { get; set; }
    }

    public record CommitNode
    {
        public CommitAuthor Author { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }
        public int ChangedFiles { get; set; }
    }

    public record CommitAuthor
    {
        public CommitUser User { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
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
        public CommitTarget Target { get; set; }
    }
}