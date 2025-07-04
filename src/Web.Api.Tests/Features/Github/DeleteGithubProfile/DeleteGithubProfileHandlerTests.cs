using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Github;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Github.DeleteGithubProfile;
using Xunit;

namespace Web.Api.Tests.Features.Github.DeleteGithubProfile;

public class DeleteGithubProfileHandlerTests
{
    [Fact]
    public async Task Delete_GithubProfileExists_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Admin).PleaseAsync(context);

        // Create test GitHub profile data
        var githubProfileData = new GithubProfileData
        {
            Name = "Test User",
            Username = "testuser",
            HtmlUrl = "https://github.com/testuser",
            Followers = 10,
            Following = 5,
            PublicRepos = 3,
            TotalPrivateRepos = 1,
            PullRequestsCreatedByUser = 2,
            IssuesOpenedByUser = 1,
            CountOfStarredRepos = 0,
            CountOfForkedRepos = 0,
            OrganizationRepoStarsAsOwner = 0
        };

        var githubProfile = new GithubProfile("testuser", githubProfileData);
        await context.SaveAsync(githubProfile);

        var allProfiles = context.GithubProfiles.ToList();
        Assert.Single(allProfiles);

        await new DeleteGithubProfileHandler(context)
            .Handle(new DeleteGithubProfileCommand("testuser"), default);

        allProfiles = context.GithubProfiles.ToList();
        Assert.Empty(allProfiles);
    }

    [Fact]
    public async Task Delete_GithubProfileDoesNotExist_ThrowsNotFoundException()
    {
        await using var context = new InMemoryDatabaseContext();

        var handler = new DeleteGithubProfileHandler(context);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteGithubProfileCommand("nonexistent"), default));

        Assert.Contains("Github profile with username 'nonexistent' not found", exception.Message);
    }
}