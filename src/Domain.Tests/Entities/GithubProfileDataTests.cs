using System;
using System.Collections.Generic;
using Domain.Entities.Github;
using Xunit;

namespace Domain.Tests.Entities;

public class GithubProfileDataTests
{
    [Fact]
    public void Constructor_SetsOrganizationRepoStarsAsOwner_Correctly()
    {
        // Arrange
        const int expectedOrgRepoStars = 150;
        
        // Act
        var profileData = new GithubProfileData(
            name: "Test User",
            username: "testuser",
            htmlUrl: "https://github.com/testuser",
            followers: 100,
            following: 50,
            publicRepos: 10,
            totalPrivateRepos: 5,
            pullRequestsCreatedByUser: 25,
            issuesOpenedByUser: 15,
            countOfStarredRepos: 75,
            countOfForkedRepos: 5,
            organizationRepoStarsAsOwner: expectedOrgRepoStars,
            commitsCount: 500,
            filesAdjusted: 200,
            changesInFilesCount: 1000,
            additionsInFilesCount: 600,
            deletionsInFilesCount: 400,
            discussionsOpened: 10,
            codeReviewsMade: 30,
            topLanguagesByCommits: new Dictionary<string, int> { { "C#", 300 }, { "JavaScript", 200 } },
            monthsToFetchCommits: 6);

        // Assert
        Assert.Equal(expectedOrgRepoStars, profileData.OrganizationRepoStarsAsOwner);
    }

    [Fact]
    public void GetTelegramFormattedText_IncludesOrganizationRepoStars()
    {
        // Arrange
        const int orgRepoStars = 250;
        var profileData = new GithubProfileData(
            name: "Test User",
            username: "testuser", 
            htmlUrl: "https://github.com/testuser",
            followers: 100,
            following: 50,
            publicRepos: 10,
            totalPrivateRepos: 5,
            pullRequestsCreatedByUser: 25,
            issuesOpenedByUser: 15,
            countOfStarredRepos: 75,
            countOfForkedRepos: 5,
            organizationRepoStarsAsOwner: orgRepoStars,
            commitsCount: 500,
            filesAdjusted: 200,
            changesInFilesCount: 1000,
            additionsInFilesCount: 600,
            deletionsInFilesCount: 400,
            discussionsOpened: 10,
            codeReviewsMade: 30,
            topLanguagesByCommits: new Dictionary<string, int>(),
            monthsToFetchCommits: 6);

        // Act
        var telegramText = profileData.GetTelegramFormattedText();

        // Assert
        Assert.Contains("Organization repo stars (as owner): <b>250</b>", telegramText);
    }

    [Fact]
    public void ObjectInitializer_SetsOrganizationRepoStarsAsOwner_Correctly()
    {
        // Arrange & Act
        var profileData = new GithubProfileData
        {
            Name = "Test User",
            Username = "testuser",
            OrganizationRepoStarsAsOwner = 300
        };

        // Assert
        Assert.Equal(300, profileData.OrganizationRepoStarsAsOwner);
    }
}