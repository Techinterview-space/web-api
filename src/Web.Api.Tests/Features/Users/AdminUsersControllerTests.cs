using System.Threading.Tasks;
using Domain.Enums;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Users;
using Web.Api.Features.Users.SearchUsersForAdmin;
using Xunit;

namespace Web.Api.Tests.Features.Users;

public class AdminUsersControllerTests
{
    [Fact]
    public async Task All_WithEmailFilter_ReturnsMatchingUsers()
    {
        await using var context = new InMemoryDatabaseContext();
        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var user1 = await new UserFake(Role.Interviewer, userName: "john.doe@example.com").PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer, userName: "jane.smith@example.com").PleaseAsync(context);

        var controller = new AdminUsersController(
            new FakeAuth(admin),
            context);

        var queryParams = new SearchUsersForAdminQueryParams
        {
            Email = "john"
        };

        context.ChangeTracker.Clear();
        var result = await controller.All(queryParams);
        
        Assert.Single(result.Results);
        Assert.Equal(user1.Id, result.Results[0].Id);
    }

    [Fact]
    public async Task All_WithUnsubscribeFilter_ReturnsMatchingUsers()
    {
        await using var context = new InMemoryDatabaseContext();
        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var user1 = await new UserFake(Role.Interviewer).WithUnsubscribeMeFromAll(true).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).WithUnsubscribeMeFromAll(false).PleaseAsync(context);

        var controller = new AdminUsersController(
            new FakeAuth(admin),
            context);

        var queryParams = new SearchUsersForAdminQueryParams
        {
            UnsubscribeMeFromAll = true
        };

        context.ChangeTracker.Clear();
        var result = await controller.All(queryParams);
        
        Assert.Single(result.Results);
        Assert.Equal(user1.Id, result.Results[0].Id);
    }

    [Fact]
    public async Task All_WithBothFilters_ReturnsMatchingUsers()
    {
        await using var context = new InMemoryDatabaseContext();
        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var user1 = await new UserFake(Role.Interviewer, userName: "john.doe@example.com").WithUnsubscribeMeFromAll(true).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer, userName: "john.smith@example.com").WithUnsubscribeMeFromAll(false).PleaseAsync(context);
        var user3 = await new UserFake(Role.Interviewer, userName: "jane.doe@example.com").WithUnsubscribeMeFromAll(true).PleaseAsync(context);

        var controller = new AdminUsersController(
            new FakeAuth(admin),
            context);

        var queryParams = new SearchUsersForAdminQueryParams
        {
            Email = "john",
            UnsubscribeMeFromAll = true
        };

        context.ChangeTracker.Clear();
        var result = await controller.All(queryParams);
        
        Assert.Single(result.Results);
        Assert.Equal(user1.Id, result.Results[0].Id);
    }

    [Fact]
    public async Task All_WithNoFilters_ReturnsAllActiveUsers()
    {
        await using var context = new InMemoryDatabaseContext();
        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var controller = new AdminUsersController(
            new FakeAuth(admin),
            context);

        var queryParams = new SearchUsersForAdminQueryParams();

        context.ChangeTracker.Clear();
        var result = await controller.All(queryParams);
        
        // Should return all 3 users (admin + 2 test users)
        Assert.Equal(3, result.Results.Count);
    }

    [Fact]
    public async Task All_WithEmptyEmailFilter_ReturnsAllActiveUsers()
    {
        await using var context = new InMemoryDatabaseContext();
        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var controller = new AdminUsersController(
            new FakeAuth(admin),
            context);

        var queryParams = new SearchUsersForAdminQueryParams
        {
            Email = ""  // Empty string should not filter
        };

        context.ChangeTracker.Clear();
        var result = await controller.All(queryParams);
        
        // Should return all users since empty email filter is ignored
        Assert.Equal(2, result.Results.Count);
    }
}