using System;
using System.Collections.Generic;
using System.Linq;

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

    public int DiscussionsOpened { get; init; }

    public int CodeReviewsMade { get; init; }

    public Dictionary<string, int> TopLanguagesByCommits { get; init; }

    public int MonthsToFetchCommits { get; init; }

    public DateTime UserCreatedAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public GithubProfileData()
    {
    }

    public GithubProfileData(
        string name,
        string username,
        string htmlUrl,
        int followers,
        int following,
        int publicRepos,
        int totalPrivateRepos,
        int pullRequestsCreatedByUser,
        int issuesOpenedByUser,
        int countOfStarredRepos,
        int countOfForkedRepos,
        int commitsCount,
        int filesAdjusted,
        int changesInFilesCount,
        int additionsInFilesCount,
        int deletionsInFilesCount,
        int discussionsOpened,
        int codeReviewsMade,
        Dictionary<string, int> topLanguagesByCommits,
        int monthsToFetchCommits = 6)
    {
        Name = name;
        Username = username;
        HtmlUrl = htmlUrl;
        Followers = followers;
        Following = following;
        PublicRepos = publicRepos;
        TotalPrivateRepos = totalPrivateRepos;
        PullRequestsCreatedByUser = pullRequestsCreatedByUser;
        IssuesOpenedByUser = issuesOpenedByUser;
        CountOfStarredRepos = countOfStarredRepos;
        CountOfForkedRepos = countOfForkedRepos;
        CommitsCount = commitsCount;
        FilesAdjusted = filesAdjusted;
        ChangesInFilesCount = changesInFilesCount;
        AdditionsInFilesCount = additionsInFilesCount;
        DeletionsInFilesCount = deletionsInFilesCount;
        DiscussionsOpened = discussionsOpened;
        CodeReviewsMade = codeReviewsMade;
        TopLanguagesByCommits = topLanguagesByCommits ?? new Dictionary<string, int>();
        MonthsToFetchCommits = monthsToFetchCommits;
        CreatedAt = DateTime.UtcNow;
    }

    public string GetTelegramFormattedText()
    {
        var languages = TopLanguagesByCommits.Count > 0
            ? string.Join(", ", TopLanguagesByCommits.Select(kvp => $"{kvp.Key} ({kvp.Value:N0})"))
            : string.Empty;

        var result = $"Github stats for <b>{Username}</b>\n" +
                     $"Name: {Name}\n" +
                     $"Profile created: <em>{UserCreatedAt:yyyy-MM-dd}</em>\n" +
                     $"<em><a href=\"{HtmlUrl}\">Profile URL</a></em>\n\n" +
                     $"🌟 Social stats:\n" +
                     $"- Followers: <b>{Followers:N0}</b>\n" +
                     $"- Following: <b>{Following:N0}</b>\n" +
                     $"- Total stars received: <b>{CountOfStarredRepos:N0}</b>\n" +
                     $"- Public Repos: <b>{PublicRepos:N0}</b>\n\n" +

                     $"- Issues created: <b>{IssuesOpenedByUser:N0}</b>\n" +
                     $"- Pull requests: <b>{PullRequestsCreatedByUser:N0}</b>\n" +
                     $"- Repositories forked: <b>{CountOfForkedRepos:N0}</b>\n" +
                     $"- Discussions opened: <b>{DiscussionsOpened:N0}</b>\n" +
                     $"- Code reviews made: <b>{CodeReviewsMade:N0}</b>\n\n" +

                     $"💻 <em>Contributions for last {MonthsToFetchCommits} month in public repositories (own and organization ones):</em>\n\n" +
                     $"- Commits: <b>{CommitsCount:N0}</b>\n" +
                     $"- Files adjusted: <b>{FilesAdjusted:N0}</b>\n" +
                     $"- Lines changed: <b>{ChangesInFilesCount:N0}</b>\n" +
                     $"- Lines added: <b>{AdditionsInFilesCount:N0}</b>\n" +
                     $"- Lines removed: <b>{DeletionsInFilesCount:N0}</b>\n";

        if (TopLanguagesByCommits.Count > 0)
        {
            result += "\n🔥 Top languages:\n";
            foreach (var kvp in TopLanguagesByCommits)
            {
                result += $"- <b>{kvp.Key}</b> ({kvp.Value:N0} commits)\n";
            }

            result += "\n";
        }

        result += $"<em>📅 Data were taken at {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC</em>\n" +
                  $"<em>Stats prepared by @github_profile_bot</em>";

        return result;
    }
}