using System;
using Domain.Entities.Github;

namespace Web.Api.Features.Telegram.GithubProfiles;

public record GithubProfileDataFromGraphQL : GithubProfileData
{
    public GithubProfileDataFromGraphQL(
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
        int deletionsInFilesCount)
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
        CreatedAt = DateTime.UtcNow;
    }
}