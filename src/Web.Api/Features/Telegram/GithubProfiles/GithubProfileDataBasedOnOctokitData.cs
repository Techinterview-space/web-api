using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Github;
using Octokit;

namespace Web.Api.Features.Telegram.GithubProfiles;

public record GithubProfileDataBasedOnOctokitData : GithubProfileData
{
    public GithubProfileDataBasedOnOctokitData(
        Octokit.User user,
        IReadOnlyList<Octokit.Repository> repositories,
        SearchIssuesResult issuesResult,
        SearchIssuesResult prsResult,
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
        Name = user.Name;
        Username = user.HtmlUrl.Split('/').LastOrDefault() ?? string.Empty;
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

        FilesAdjusted = filesAdjusted;
        IssuesOpenedByUser = issuesResult.TotalCount;
        PullRequestsCreatedByUser = prsResult.TotalCount;
        CommitsCount = commitsCount;
        ChangesInFilesCount = changesInFilesCount;
        AdditionsInFilesCount = additionsInFilesCount;
        DeletionsInFilesCount = deletionsInFilesCount;
        DiscussionsOpened = discussionsOpened;
        CodeReviewsMade = codeReviewsMade;
        TopLanguagesByCommits = topLanguagesByCommits ?? new Dictionary<string, int>();
        MonthsToFetchCommits = monthsToFetchCommits;
        CreatedAt = DateTime.UtcNow;
    }
}