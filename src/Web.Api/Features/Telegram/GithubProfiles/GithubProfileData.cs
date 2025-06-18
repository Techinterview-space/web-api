using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace Web.Api.Features.Telegram.GithubProfiles;

public record GithubProfileData
{
    public string Name { get; init; }

    public string Username => HtmlUrl.Split('/').LastOrDefault() ?? string.Empty;

    public string HtmlUrl { get; init; }

    public int Followers { get; init; }

    public int Following { get; init; }

    public int PublicRepos { get; init; }

    public int TotalPrivateRepos { get; init; }

    public int PullRequestsCreatedByUser { get; init; }

    public int IssuesOpenedByUser { get; init; }

    public int CountOfStarredRepos { get; init; }

    public int CountOfForkedRepos { get; init; }

    public GithubProfileData()
    {
    }

    public GithubProfileData(
        Octokit.User user,
        IReadOnlyList<Octokit.Repository> repositories,
        SearchIssuesResult issuesResult,
        SearchIssuesResult prsResult)
    {
        Name = user.Name;
        HtmlUrl = user.HtmlUrl;
        Followers = user.Followers;
        Following = user.Following;
        PublicRepos = user.PublicRepos;
        TotalPrivateRepos = user.TotalPrivateRepos;

        CountOfStarredRepos = repositories.Count > 0
            ? repositories
                .Sum(r => r.StargazersCount)
            : 0;

        CountOfForkedRepos = repositories.Count(r => r.Fork);

        IssuesOpenedByUser = issuesResult.TotalCount;
        PullRequestsCreatedByUser = prsResult.TotalCount;
    }

    public string GetTelegramFormattedText()
    {
        return $"Github stats for <b>{Username}</b> ({Name}):\n" +
               $"<em><a href=\"{HtmlUrl}\">Profile URL</a></em>\n\n" +
               $"Followers: <b>{Followers}</b>\n" +
               $"Following: <b>{Following}</b>\n" +
               $"Public Repos: <b>{PublicRepos}</b>\n\n" +
               $"Issues created: <b>{IssuesOpenedByUser}</b>\n" +
               $"Pull requests: <b>{PullRequestsCreatedByUser}</b>\n" +
               $"Total stars received: <b>{CountOfStarredRepos}</b>\n" +
               $"Repositories forked: <b>{CountOfForkedRepos}</b>\n\n" +
               $"<em>Stats sent by @github_profile_bot</em>";
    }

}