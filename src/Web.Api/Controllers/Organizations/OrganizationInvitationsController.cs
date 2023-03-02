using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Attributes;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Emails.Services;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services.Organizations;
using MG.Utils.EFCore;
using MG.Utils.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TechInterviewer.Controllers.Organizations;

[ApiController]
[Route("api/organization-invitations")]
[HasAnyRole]
public class OrganizationInvitationsController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;
    private readonly IEmailService _emailService;

    public OrganizationInvitationsController(
        IAuthorization auth,
        DatabaseContext context,
        IEmailService emailService)
    {
        _auth = auth;
        _context = context;
        _emailService = emailService;
    }

    [HttpGet("for-me")]
    public async Task<IEnumerable<JoinToOrgInvitationDto>> ForMeAsync()
    {
        var currentUser = await _auth.CurrentUserAsync();
        return await _context.JoinToOrgInvitations
            .Include(o => o.Organization)
            .Include(x => x.Inviter)
            .Where(x => x.InvitedUserId == currentUser.Id && x.Status == InvitationStatus.Pending)
            .AllAsync(x => new JoinToOrgInvitationDto(x));
    }

    [HttpGet("for-organization/{organizationId:guid}")]
    public async Task<IEnumerable<JoinToOrgInvitationDto>> ForOrganizationAsync(
        [FromRoute] Guid organizationId)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var organization = await _context.Organizations
            .Include(o => o.Invitations)
            .ByIdOrFailAsync(organizationId);

        if (!currentUser.Has(Role.Admin) && organization.ManagerId != currentUser.Id)
        {
            throw new NoPermissionsException();
        }

        return organization
            .Invitations
            .Select(x => new JoinToOrgInvitationDto(x));
    }

    [HttpPost("invite-user")]
    public async Task<IActionResult> InviteUserAsync(
        [FromBody] InviteUserToOrganizationRequest request)
    {
        var currentUser = await _auth.CurrentUserAsync();
        if (!currentUser.IsMyOrganization(request.OrganizationId))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var organization = await _context.Organizations
            .Include(x => x.Users)
            .Include(x => x.Invitations)
            .ByIdOrFailAsync(request.OrganizationId);

        var user = await _context.Users.ByEmailOrNullAsync(request.Email);

        if (user is null)
        {
            return NotFound("User with this email does not exist");
        }

        if (organization.HasUserBeenInvited(user))
        {
            return BadRequest("User is already invited to this organization");
        }

        if (organization.HasUser(user))
        {
            return BadRequest("User is already a member of this organization");
        }

        if (currentUser.Id == user.Id)
        {
            return BadRequest("You cannot invite yourself to the organization");
        }

        var entry = await _context.JoinToOrgInvitations.AddAsync(new JoinToOrgInvitation(organization, user, currentUser));
        await _context.TrySaveChangesAsync();

        await _emailService.InvitationAsync(
            organization,
            invitedPerson: user,
            inviter: currentUser);

        return Ok(entry.Entity.Id);
    }

    [HttpDelete("{invitationId:guid}")]
    public async Task<IActionResult> DeleteInvitationAsync(
        [FromRoute] Guid invitationId)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var invitation = await _context.JoinToOrgInvitations
            .Include(x => x.Organization)
            .ByIdOrFailAsync(invitationId);

        if (!invitation.IsInviter(currentUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        _context.Remove(invitation);
        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpPost("{invitationId:guid}/accept")]
    public async Task<IActionResult> AcceptInvitationAsync(
        [FromRoute] Guid invitationId)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var invitation = await _context.JoinToOrgInvitations
            .Include(x => x.Organization)
            .Include(x => x.InvitedUser)
            .Include(x => x.Inviter)
            .ByIdOrFailAsync(invitationId);

        if (!invitation.IsInvitedPerson(currentUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var organization = invitation.Organization;

        if (organization.HasUser(invitation.InvitedUserId))
        {
            if (invitation.Status is InvitationStatus.Pending)
            {
                _context.Remove(invitation);
                await _context.TrySaveChangesAsync();
            }

            return BadRequest("User is already in this organization");
        }

        if (invitation.Status is InvitationStatus.Accepted or InvitationStatus.Declined)
        {
            throw new InvalidOperationException($"Invitation is already {invitation.Status}");
        }

        organization.Users.Add(new OrganizationUser(invitation.InvitedUser, organization));

        _context.Remove(invitation);
        await _context.TrySaveChangesAsync();
        await _emailService.InvitationAcceptedAsync(
            organization,
            invitation.InvitedUser,
            invitation.Inviter);

        return Ok();
    }

    [HttpPost("{invitationId:guid}/reject")]
    public async Task<IActionResult> DeclineInvitationAsync(
        [FromRoute] Guid invitationId)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var invitation = await _context.JoinToOrgInvitations
            .Include(x => x.Organization)
            .Include(x => x.InvitedUser)
            .Include(x => x.Inviter)
            .ByIdOrFailAsync(invitationId);

        if (!invitation.IsInvitedPerson(currentUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        if (invitation.Organization.HasUser(invitation.InvitedUserId))
        {
            if (invitation.Status is InvitationStatus.Pending)
            {
                _context.Remove(invitation);
                await _context.TrySaveChangesAsync();
            }

            return BadRequest("User is already in this organization");
        }

        invitation.Decline();
        await _context.TrySaveChangesAsync();

        await _emailService.InvitationDeclinedAsync(
            invitation.Organization,
            invitation.InvitedUser,
            invitation.Inviter);
        return Ok();
    }
}