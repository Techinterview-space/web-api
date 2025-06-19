using System;

namespace Domain.Entities.Github;

public record GithubProfileData
{
    public string Name { get; init; }

    public string Username { get; init; }

    public string HtmlUrl { get; init; }

    public int Followers { get; init; }

    public int Following { get; init; }

    public int PublicRepos { get; init; }

    public int TotalPrivateRepos { get; init; }

    public int PullRequestsCreatedByUser { get; init; }

    public int IssuesOpenedByUser { get; init; }

    public int CountOfStarredRepos { get; init; }

    public int CountOfForkedRepos { get; init; }

    public int CommitsCount { get; init; }

    public int FilesAdjusted { get; init; }

    public int ChangesInFilesCount { get; init; }

    public int AdditionsInFilesCount { get; init; }

    public int DeletionsInFilesCount { get; init; }

    public DateTime CreatedAt { get; init; }

    public GithubProfileData()
    {
    }

    public string GetTelegramFormattedText(
        int month = 3)
    {
        return $"Github stats for <b>{Username}</b> ({Name}):\n" +
               $"<em><a href=\"{HtmlUrl}\">Profile URL</a></em>\n\n" +
               $"Followers: <b>{Followers:N0}</b>\n" +
               $"Following: <b>{Following:N0}</b>\n" +
               $"Public Repos: <b>{PublicRepos:N0}</b>\n\n" +
               $"Issues created: <b>{IssuesOpenedByUser:N0}</b>\n" +
               $"Pull requests: <b>{PullRequestsCreatedByUser:N0}</b>\n" +
               $"Total stars received: <b>{CountOfStarredRepos:N0}</b>\n" +
               $"Repositories forked: <b>{CountOfForkedRepos:N0}</b>\n\n" +
               $"<em>Stats for last {month} month in public repositories (own and organization ones):</em>\n\n" +
               $"Commits made: <b>{CommitsCount:N0}</b>\n" +
               $"Files adjusted: <b>{FilesAdjusted:N0}</b>\n" +
               $"Lines changed: <b>{ChangesInFilesCount:N0}</b>\n" +
               $"Lines added: <b>{AdditionsInFilesCount:N0}</b>\n" +
               $"Lines removed: <b>{DeletionsInFilesCount:N0}</b>\n\n" +
               $"<em>Data were taken at {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC</em>\n" +
               $"<em>Stats prepared by @github_profile_bot</em>";
    }
}