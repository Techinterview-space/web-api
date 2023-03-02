using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Services.Organizations;
using MG.Utils.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Controllers.Organizations;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace Web.Api.Tests.Controllers;

public class OrganizationsControllerTests
{
    [Fact]
    public async Task MyAsync_OnlyMyReturns_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization1 = await new FakeOrganization()
            .WithUser(user)
            .PleaseAsync(context);

        var organization2 = await new FakeOrganization()
            .PleaseAsync(context);

        var organization3 = await new FakeOrganization()
            .WithUser(user)
            .PleaseAsync(context);

        var myOrgs = (await controller.MyAsync()).ToArray();
        Assert.Equal(2, myOrgs.Length);
        Assert.Contains(myOrgs, x => x.Id == organization1.Id);
        Assert.Contains(myOrgs, x => x.Id == organization3.Id);
        Assert.DoesNotContain(myOrgs, x => x.Id == organization2.Id);
    }

    [Fact]
    public async Task MyForSelectBoxesAsync_OnlyMyReturns_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization1 = await new FakeOrganization()
            .WithUser(user)
            .PleaseAsync(context);

        var organization2 = await new FakeOrganization()
            .PleaseAsync(context);

        var organization3 = await new FakeOrganization()
            .WithUser(user)
            .PleaseAsync(context);

        var myOrgs = (await controller.MyForSelectBoxesAsync()).ToArray();
        Assert.Equal(2, myOrgs.Length);
        Assert.Contains(myOrgs, x => x.Id == organization1.Id);
        Assert.Contains(myOrgs, x => x.Id == organization3.Id);
        Assert.DoesNotContain(myOrgs, x => x.Id == organization2.Id);
    }

    [Fact]
    public async Task CreatedByMeAsync_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization1 = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var organization2 = await new FakeOrganization(user)
            .PleaseAsync(context);

        var organization3 = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var myOrgs = (await controller.CreatedByMeAsync()).ToArray();
        Assert.Equal(3, myOrgs.Length);
        Assert.Contains(myOrgs, x => x.Id == organization1.Id);
        Assert.Contains(myOrgs, x => x.Id == organization3.Id);
        Assert.Contains(myOrgs, x => x.Id == organization2.Id);
    }

    [Fact]
    public async Task ByIdAsync_CreatedByMe_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization(user)
            .PleaseAsync(context);

        var dto = await controller.ByIdAsync(organization.Id);
        Assert.Equal(organization.Id, dto.Id);
    }

    [Fact]
    public async Task ByIdAsync_MeAsMember_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .WithUser(user)
            .PleaseAsync(context);

        var dto = await controller.ByIdAsync(organization.Id);
        Assert.Equal(organization.Id, dto.Id);
    }

    [Fact]
    public async Task ByIdAsync_MeAsAdmin_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .PleaseAsync(context);

        var dto = await controller.ByIdAsync(organization.Id);
        Assert.Equal(organization.Id, dto.Id);
    }

    [Fact]
    public async Task ByIdAsync_MeAsNobodyForTheOrg_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .PleaseAsync(context);

        var dto = await controller.ByIdAsync(organization.Id);
        Assert.Equal(organization.Id, dto.Id);
    }

    [Fact]
    public async Task AttachUserToOrganizationAsync_MeAsAdmin_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .WithUser(user)
            .PleaseAsync(context);

        Assert.Empty(organization.Invitations);
        Assert.Single(organization.Users);

        var result = await controller.AttachUserToOrganizationAsync(
            organization.Id,
            otherPerson.Id);

        Assert.True(result is OkResult);
        var org = await controller.ByIdAsync(organization.Id);

        Assert.Equal(2, org.Users.Count());
        Assert.Empty(org.Invitations);
    }

    [Fact]
    public async Task AttachUserToOrganizationAsync_WasInvited_InvitationRemovedAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .WithUser(user)
            .WithInvitation(otherPerson, user)
            .PleaseAsync(context);

        Assert.Single(organization.Invitations);
        Assert.Single(organization.Users);

        var result = await controller.AttachUserToOrganizationAsync(
            organization.Id,
            otherPerson.Id);

        Assert.True(result is OkResult);
        var org = await controller.ByIdAsync(organization.Id);

        Assert.Equal(2, org.Users.Count());
        Assert.Empty(org.Invitations);
    }

    [Fact]
    public async Task AttachUserToOrganizationAsync_UserAttached_ExceptionAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .WithUser(user)
            .WithUser(otherPerson)
            .PleaseAsync(context);

        Assert.Empty(organization.Invitations);
        Assert.Equal(2, organization.Users.Count);

        var result = await controller.AttachUserToOrganizationAsync(
            organization.Id,
            otherPerson.Id);

        Assert.True(result is BadRequestObjectResult);
        var org = await controller.ByIdAsync(organization.Id);

        Assert.Equal(2, org.Users.Count());
        Assert.Empty(org.Invitations);
    }

    [Fact]
    public async Task ExcludeUserFromOrganizationAsync_UserAttached_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .WithUser(user)
            .WithUser(otherPerson)
            .PleaseAsync(context);

        Assert.Empty(organization.Invitations);
        Assert.Equal(2, organization.Users.Count);

        var result = await controller.ExcludeUserFromOrganizationAsync(
            organization.Id,
            otherPerson.Id);

        Assert.True(result is OkResult);
        var org = await controller.ByIdAsync(organization.Id);

        Assert.Single(org.Users);
    }

    [Fact]
    public async Task ExcludeUserFromOrganizationAsync_UserWasNotAttached_ExceptionAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .WithUser(user)
            .PleaseAsync(context);

        Assert.Single(organization.Users);

        var result = await controller.ExcludeUserFromOrganizationAsync(
            organization.Id,
            otherPerson.Id);

        Assert.True(result is BadRequestObjectResult);
        var org = await controller.ByIdAsync(organization.Id);

        Assert.Single(org.Users);
    }

    [Fact]
    public async Task DeleteAsync_WasActive_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .WithUser(user)
            .WithInvitation(otherPerson, user)
            .PleaseAsync(context);

        Assert.Single(organization.Users);
        Assert.Single(organization.Invitations);
        Assert.Null(organization.DeletedAt);

        var result = await controller.DeleteAsync(organization.Id);

        Assert.Equal(typeof(OkResult), result.GetType());
        var org = await controller.ByIdAsync(organization.Id);

        Assert.Single(org.Users);
        Assert.Empty(org.Invitations);
        Assert.NotNull(org.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_WasInactive_BadResultAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Admin).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .MakeInactive()
            .WithUser(user)
            .WithInvitation(otherPerson, user)
            .PleaseAsync(context);

        Assert.Single(organization.Users);
        Assert.Single(organization.Invitations);
        Assert.NotNull(organization.DeletedAt);

        var result = await controller.DeleteAsync(organization.Id);

        Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
        var org = await controller.ByIdAsync(organization.Id);

        Assert.Single(org.Users);
        Assert.Single(org.Invitations);
        Assert.NotNull(org.DeletedAt);
    }

    [Fact]
    public async Task LeaveOrganizationAsync_MeAsParticipant_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization()
            .WithUser(user)
            .WithUser(otherPerson)
            .PleaseAsync(context);

        Assert.Empty(organization.Invitations);
        Assert.Equal(2, organization.Users.Count);

        var result = await controller.LeaveOrganizationAsync(
            organization.Id,
            new LeaveOrganizationRequest
            {
                NewManagerId = null,
            });

        Assert.Equal(typeof(OkResult), result.GetType());
        organization = await context.Organizations
            .Include(x => x.Users)
            .ByIdOrFailAsync(organization.Id);

        Assert.Single(organization.Users);
        Assert.DoesNotContain(organization.Users, x => x.UserId == user.Id);
    }

    [Fact]
    public async Task LeaveOrganizationAsync_MeAsManager_OtherPersonBecomesManager_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization(user)
            .WithUser(otherPerson)
            .PleaseAsync(context);

        Assert.Empty(organization.Invitations);
        Assert.Equal(2, organization.Users.Count);

        var result = await controller.LeaveOrganizationAsync(
            organization.Id,
            new LeaveOrganizationRequest
            {
                NewManagerId = otherPerson.Id,
            });

        Assert.Equal(typeof(OkResult), result.GetType());
        organization = await context.Organizations
            .Include(x => x.Users)
            .ByIdOrFailAsync(organization.Id);

        Assert.Single(organization.Users);
        Assert.DoesNotContain(organization.Users, x => x.UserId == user.Id);
        Assert.Contains(organization.Users, x => x.UserId == otherPerson.Id);
        Assert.Equal(otherPerson.Id, organization.ManagerId);
    }

    [Fact]
    public async Task LeaveOrganizationAsync_MeAsManager_NoNewManager_ExceptionAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization(user)
            .WithUser(otherPerson)
            .PleaseAsync(context);

        Assert.Empty(organization.Invitations);
        Assert.Equal(2, organization.Users.Count);

        var result = await controller.LeaveOrganizationAsync(
            organization.Id,
            new LeaveOrganizationRequest
            {
                NewManagerId = null,
            });

        Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
        organization = await context.Organizations
            .Include(x => x.Users)
            .ByIdOrFailAsync(organization.Id);

        Assert.Equal(2, organization.Users.Count);
        Assert.Contains(organization.Users, x => x.UserId == user.Id);
        Assert.Contains(organization.Users, x => x.UserId == otherPerson.Id);
        Assert.Equal(user.Id, organization.ManagerId);
    }

    [Fact]
    public async Task LeaveOrganizationAsync_MeAsManager_NoOtherPersons_ExceptionAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationsController(new FakeAuth(user), context);

        var organization = await new FakeOrganization(user)
            .PleaseAsync(context);

        Assert.Empty(organization.Invitations);
        Assert.Single(organization.Users);

        var result = await controller.LeaveOrganizationAsync(
            organization.Id,
            new LeaveOrganizationRequest
            {
                NewManagerId = null,
            });

        Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
        organization = await context.Organizations
            .Include(x => x.Users)
            .ByIdOrFailAsync(organization.Id);

        Assert.Single(organization.Users);
        Assert.Contains(organization.Users, x => x.UserId == user.Id);
        Assert.Equal(user.Id, organization.ManagerId);
    }
}