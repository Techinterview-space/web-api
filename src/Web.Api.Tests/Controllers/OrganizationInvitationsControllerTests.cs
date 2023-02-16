using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Organizations;
using Domain.Enums;
using Domain.Services.Organizations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechInterviewer.Controllers.Organizations;
using TestUtils;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace Web.Api.Tests.Controllers;

public class OrganizationInvitationsControllerTests
{
    [Fact]
    public async Task ForMeAsync_OnlyMyReturns_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var otherUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization1 = await new FakeOrganization(user)
            .WithUser(user)
            .WithInvitation(otherUser, user)
            .PleaseAsync(context);

        var organization2 = await new FakeOrganization()
            .WithInvitation(otherUser, user)
            .PleaseAsync(context);

        var organization3 = await new FakeOrganization()
            .WithUser(user)
            .PleaseAsync(context);

        var invitations = (await new OrganizationInvitationsController(
            new FakeAuth(otherUser), context, new EmailServiceMock().Object).ForMeAsync()).ToArray();
        Assert.Equal(2, invitations.Length);
        Assert.Contains(invitations, x => x.OrganizationId == organization1.Id);
        Assert.Contains(invitations, x => x.OrganizationId == organization2.Id);
        Assert.DoesNotContain(invitations, x => x.OrganizationId == organization3.Id);
    }

    [Fact]
    public async Task ForOrganizationAsync_OnlyMyReturns_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var otherUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationInvitationsController(
            new FakeAuth(user), context, new EmailServiceMock().Object);

        var organization1 = await new FakeOrganization(user)
            .WithUser(user)
            .WithInvitation(otherUser, user)
            .PleaseAsync(context);

        var organization2 = await new FakeOrganization()
            .WithInvitation(otherUser, user)
            .PleaseAsync(context);

        var organization3 = await new FakeOrganization()
            .WithUser(user)
            .PleaseAsync(context);

        var invitations = (await controller.ForOrganizationAsync(organization1.Id)).ToArray();
        Assert.Single(invitations);
        Assert.Contains(invitations, x => x.OrganizationId == organization1.Id);
        Assert.DoesNotContain(invitations, x => x.OrganizationId == organization2.Id);
        Assert.DoesNotContain(invitations, x => x.OrganizationId == organization3.Id);
    }

    [Fact]
    public async Task ForOrganizationAsync_MeAsAdmin_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var orgUser = await new FakeUser(Role.Admin).PleaseAsync(context);
        var otherUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationInvitationsController(
            new FakeAuth(user), context, new EmailServiceMock().Object);

        var organization1 = await new FakeOrganization()
            .WithUser(orgUser)
            .WithInvitation(otherUser, orgUser)
            .PleaseAsync(context);

        var organization2 = await new FakeOrganization()
            .WithInvitation(otherUser, orgUser)
            .PleaseAsync(context);

        var organization3 = await new FakeOrganization()
            .WithUser(orgUser)
            .PleaseAsync(context);

        var invitations = (await controller.ForOrganizationAsync(organization1.Id)).ToArray();
        Assert.Single(invitations);
        Assert.Contains(invitations, x => x.OrganizationId == organization1.Id);
        Assert.DoesNotContain(invitations, x => x.OrganizationId == organization2.Id);
        Assert.DoesNotContain(invitations, x => x.OrganizationId == organization3.Id);
    }

    [Fact]
    public async Task InviteUserAsync_NoUserInOrg_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var otherUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var email = new EmailServiceMock();
        var controller = new OrganizationInvitationsController(
            new FakeAuth(user), context, email.Object);

        var organization1 = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var result = await controller.InviteUserAsync(new InviteUserToOrganizationRequest
        {
            OrganizationId = organization1.Id,
            Email = otherUser.Email,
        });

        Assert.Equal(typeof(OkObjectResult), result.GetType());
        var invitationId = ((OkObjectResult)result).Value as Guid?;
        var invitation = await context.JoinToOrgInvitations.ByIdOrFailAsync(invitationId!.Value);

        Assert.Equal(otherUser.Id, invitation.InvitedUserId);
        Assert.Equal(user.Id, invitation.InviterId);
        Assert.Equal(organization1.Id, invitation.OrganizationId);
        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        email.VerifyInvitationSent(Times.Once);
    }

    [Fact]
    public async Task InviteUserAsync_UserWasInvitedAlready_BadResultAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var otherUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationInvitationsController(
            new FakeAuth(user), context, new EmailServiceMock().Object);

        var organization1 = await new FakeOrganization(user)
            .WithUser(user)
            .WithInvitation(otherUser, user)
            .PleaseAsync(context);

        var result = await controller.InviteUserAsync(new InviteUserToOrganizationRequest
        {
            OrganizationId = organization1.Id,
            Email = otherUser.Email,
        });

        Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
    }

    [Fact]
    public async Task InviteUserAsync_UserWasIncludedAlready_BadResultAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var otherUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var controller = new OrganizationInvitationsController(
            new FakeAuth(user), context, new EmailServiceMock().Object);

        var organization1 = await new FakeOrganization(user)
            .WithUser(user)
            .WithUser(otherUser)
            .PleaseAsync(context);

        var result = await controller.InviteUserAsync(new InviteUserToOrganizationRequest
        {
            OrganizationId = organization1.Id,
            Email = otherUser.Email,
        });

        Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
    }

    [Fact]
    public async Task InviteUserAsync_InvitingMyself_BadResultAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var email = new EmailServiceMock();
        var controller = new OrganizationInvitationsController(
            new FakeAuth(user), context, email.Object);

        var organization1 = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var result = await controller.InviteUserAsync(new InviteUserToOrganizationRequest
        {
            OrganizationId = organization1.Id,
            Email = user.Email,
        });

        Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
        email.VerifyInvitationSent(Times.Never);
    }

    [Fact]
    public async Task InviteUserAsync_NoUser_NotFoundResultAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var email = new EmailServiceMock();
        var controller = new OrganizationInvitationsController(
            new FakeAuth(user), context, email.Object);

        var organization1 = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var result = await controller.InviteUserAsync(new InviteUserToOrganizationRequest
        {
            OrganizationId = organization1.Id,
            Email = user.Email + "123",
        });

        Assert.Equal(typeof(NotFoundObjectResult), result.GetType());
        email.VerifyInvitationSent(Times.Never);
    }

    [Fact]
    public async Task AcceptInvitationAsync_Valid_OkResultAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var invited = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var email = new EmailServiceMock();
        var controller = new OrganizationInvitationsController(
            new FakeAuth(user), context, email.Object);

        var organization = await new FakeOrganization(user).PleaseAsync(context);

        var result = await controller.InviteUserAsync(new InviteUserToOrganizationRequest
        {
            OrganizationId = organization.Id,
            Email = invited.Email,
        });

        Assert.Equal(typeof(OkObjectResult), result.GetType());
        Assert.Equal(1, await context.JoinToOrgInvitations.CountAsync());
        organization = await context.Organizations
            .Include(x => x.Users)
            .Include(x => x.Invitations)
            .ByIdOrFailAsync(organization.Id);

        Assert.Single(organization.Users);
        Assert.Single(organization.Invitations);
        var invitationId = ((OkObjectResult)result).Value as Guid?;

        var target = new OrganizationInvitationsController(
            new FakeAuth(invited), context, email.Object);
        var acceptResult = await target.AcceptInvitationAsync(invitationId!.Value);
        Assert.Equal(typeof(OkResult), acceptResult.GetType());

        organization = await context.Organizations
            .Include(x => x.Users)
            .Include(x => x.Invitations)
            .ByIdOrFailAsync(organization.Id);

        Assert.Equal(2, organization.Users.Count);
        Assert.Empty(organization.Invitations);
        email.VerifyInvitationAccepted(Times.Once);
    }

    [Fact]
    public async Task DeclineInvitationAsync_Valid_OkResultAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var invited = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var email = new EmailServiceMock();
        var controller = new OrganizationInvitationsController(
            new FakeAuth(user), context, email.Object);

        var organization = await new FakeOrganization(user).PleaseAsync(context);

        var result = await controller.InviteUserAsync(new InviteUserToOrganizationRequest
        {
            OrganizationId = organization.Id,
            Email = invited.Email,
        });

        Assert.Equal(typeof(OkObjectResult), result.GetType());
        Assert.Equal(1, await context.JoinToOrgInvitations.CountAsync());
        organization = await context.Organizations
            .Include(x => x.Users)
            .Include(x => x.Invitations)
            .ByIdOrFailAsync(organization.Id);

        Assert.Single(organization.Users);
        Assert.Single(organization.Invitations);
        var invitationId = ((OkObjectResult)result).Value as Guid?;

        var target = new OrganizationInvitationsController(
            new FakeAuth(invited), context, email.Object);
        var acceptResult = await target.DeclineInvitationAsync(invitationId!.Value);
        Assert.Equal(typeof(OkResult), acceptResult.GetType());

        organization = await context.Organizations
            .Include(x => x.Users)
            .Include(x => x.Invitations)
            .ByIdOrFailAsync(organization.Id);

        Assert.Single(organization.Users);
        Assert.Single(organization.Invitations);
        Assert.Equal(InvitationStatus.Declined, organization.Invitations.First().Status);
        email.VerifyInvitationDeclined(Times.Once);
    }
}