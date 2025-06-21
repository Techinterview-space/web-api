using System;
using System.Collections.Generic;
using Domain.Entities.Github;

namespace Web.Api.Features.Telegram.GithubProfiles;

public record GithubProfileDataFromGraphQl : GithubProfileData
{
    public GithubProfileDataFromGraphQl(
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
        Dictionary<string, int> topLanguagesByCommits)
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
        CreatedAt = DateTime.UtcNow;
    }
}