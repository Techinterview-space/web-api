using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Attributes;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services.Interviews.Dtos;
using Domain.Services.InterviewTemplates;
using Domain.Services.Organizations;
using Domain.Services.Organizations.Requests;
using MG.Utils.EFCore;
using MG.Utils.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TechInterviewer.Controllers.Organizations;

[ApiController]
[Route("api/organizations")]
[HasAnyRole]
public class OrganizationsController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public OrganizationsController(IAuthorization auth, DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("")]
    [HasAnyRole(Role.Admin)]
    public async Task<IEnumerable<OrganizationDto>> AllAsync()
    {
        return await _context.Organizations
            .Include(x => x.Candidates)
            .Include(x => x.Manager)
            .Include(x => x.Users)
            .AllAsync(x => new OrganizationDto(x));
    }

    [HttpGet("for-select-boxes")]
    [HasAnyRole(Role.Admin)]
    public async Task<IEnumerable<OrganizationSimpleDto>> AllForSelectBoxesAsync()
    {
        return await _context.Organizations
            .Where(x => x.DeletedAt == null)
            .Select(x => new OrganizationSimpleDto
            {
                Id = x.Id,
                Name = x.Name,
            })
            .AllAsync();
    }

    [HttpGet("my")]
    public async Task<IEnumerable<OrganizationDto>> MyAsync()
    {
        var myOrganizations = await _auth.MyOrganizationsAsync();

        return await _context.Organizations
            .Include(x => x.Manager)
            .Where(x => myOrganizations.Contains(x.Id))
            .AllAsync(x => new OrganizationDto(x));
    }

    [HttpGet("my/for-select-boxes")]
    public async Task<IEnumerable<OrganizationSimpleDto>> MyForSelectBoxesAsync()
    {
        var myOrganizations = await _auth.MyOrganizationsAsync();
        return await _context.Organizations
            .Where(x => myOrganizations.Contains(x.Id))
            .Where(x => x.DeletedAt == null)
            .Select(x => new OrganizationSimpleDto
            {
                Id = x.Id,
                Name = x.Name,
            })
            .AllAsync();
    }

    [HttpGet("created-by-me")]
    public async Task<IEnumerable<OrganizationDto>> CreatedByMeAsync()
    {
        var currentUser = await _auth.CurrentUserAsync();

        return await _context.Organizations
            .Where(x => x.ManagerId == currentUser.Id)
            .AllAsync(x => new OrganizationDto(x));
    }

    [HttpGet("{id:guid}")]
    public async Task<OrganizationDto> ByIdAsync([FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();

        var query = _context.Organizations
            .Include(x => x.Manager);

        if (currentUser.Has(Role.Admin) || currentUser.IsMyOrganization(id))
        {
            query = query
                .Include(x => x.Users)
                .ThenInclude(x => x.User)
                .Include(x => x.Invitations)
                .ThenInclude(x => x.InvitedUser);
        }

        var organization = await query
            .AsNoTracking()
            .ByIdOrFailAsync(id);

        return new OrganizationDto(organization);
    }

    [HttpGet("{id:guid}/simple")]
    public async Task<IActionResult> ByIdSimpleAsync([FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();
        if (currentUser.Has(Role.Admin) || currentUser.IsMyOrganization(id))
        {
            return Ok(new OrganizationSimpleDto(
                await _context.Organizations
                    .AsNoTracking()
                    .ByIdOrFailAsync(id)));
        }

        return Forbid();
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateOrganizationRequest request)
    {
        var currentUser = await _auth.CurrentUserAsync();

        var entry = await _context.Organizations.AddAsync(new Organization(
            request.Name,
            request.Description,
            currentUser));

        await _context.TrySaveChangesAsync();
        return Ok(entry.Entity.Id);
    }

    [HttpPut("")]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateOrganizationRequest request)
    {
        var currentUser = await _auth.CurrentUserAsync();

        var organization = await _context.Organizations.ByIdOrFailAsync(request.Id);
        if (!organization.CouldBeModifiedBy(currentUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        organization.Update(request.Name, request.Description);
        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpPost("{organizationId:guid}/leave")]
    public async Task<IActionResult> LeaveOrganizationAsync(
        [FromRoute] Guid organizationId,
        [FromBody] LeaveOrganizationRequest request)
    {
        var organization = await _context.Organizations
            .Include(x => x.Users)
            .ByIdOrFailAsync(organizationId);

        var currentUser = await _auth.CurrentUserAsync();
        if (!organization.HasUser(currentUser))
        {
            return BadRequest("User is not a member of this organization");
        }

        if (organization.IsManagerBy(currentUser))
        {
            if (organization.Users.Count == 1)
            {
                return BadRequest("If you are the only user in this organization, it would be better to remove it.");
            }

            if (request.NewManagerId == null)
            {
                return BadRequest("New manager id is required");
            }

            var newManager = await _context.Users.ByIdOrFailAsync(request.NewManagerId.Value);
            organization.ChangeManager(newManager);
        }

        organization.ExcludeUser(currentUser.Id);
        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpPost("{organizationId:guid}/attach-user/{userId:long}")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> AttachUserToOrganizationAsync(
        [FromRoute] Guid organizationId,
        [FromRoute] long userId)
    {
        var organization = await _context.Organizations
            .Include(x => x.Users)
            .ByIdOrFailAsync(organizationId);

        var user = await _context.Users.ByIdOrFailAsync(userId);

        if (organization.HasUser(user))
        {
            return BadRequest("User is already attached to this organization");
        }

        if (organization.HasInvitationFor(user))
        {
            _context.Remove(organization.GetInvitationFor(user));
        }

        organization.AttachUser(user);
        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpPost("{organizationId:guid}/exclude-user/{userId:long}")]
    public async Task<IActionResult> ExcludeUserFromOrganizationAsync(
        [FromRoute] Guid organizationId,
        [FromRoute] long userId)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var organization = await _context.Organizations
            .Include(x => x.Users)
            .ByIdOrFailAsync(organizationId);

        if (!currentUser.Has(Role.Admin) && !organization.IsManagerBy(currentUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        if (!organization.HasUser(userId))
        {
            return BadRequest("User was not attached to this organization");
        }

        if (currentUser.Id == userId)
        {
            return BadRequest("You cannot exclude yourself from an organization");
        }

        organization.ExcludeUser(userId);
        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();

        var organization = await _context.Organizations
            .Include(x => x.Invitations)
            .Include(x => x.Candidates)
            .ByIdOrFailAsync(id);

        if (!organization.CouldBeModifiedBy(currentUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        if (!organization.Active)
        {
            return BadRequest("Organization is already deleted");
        }

        organization.Archive();
        _context.RemoveRangeIfNotEmpty(organization.Invitations);

        foreach (var candidate in organization.Candidates)
        {
            if (candidate.Active)
            {
                candidate.Archive();
            }
        }

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:guid}/remove")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> RemoveAsync([FromRoute] Guid id)
    {
        var organization = await _context.Organizations
            .Include(x => x.Users)
            .Include(x => x.Invitations)
            .Include(x => x.Candidates)
            .ThenInclude(x => x.CandidateCards)
            .ThenInclude(x => x.Interviews)
            .ByIdOrFailAsync(id);

        if (organization.Active)
        {
            return BadRequest("The organization is active");
        }

        _context.Remove(organization);
        _context.RemoveRangeIfNotEmpty(organization.Users);
        _context.RemoveRangeIfNotEmpty(organization.Candidates);
        _context.RemoveRangeIfNotEmpty(organization.Invitations);

        _context.RemoveRangeIfNotEmpty(await _context.OrganizationLabels
            .Where(x => x.OrganizationId == organization.Id)
            .ToArrayAsync());

        var candidateCards = organization.Candidates.SelectMany(x => x.CandidateCards).ToArray();
        _context.RemoveRangeIfNotEmpty(candidateCards);

        var interviews = candidateCards.SelectMany(x => x.Interviews).ToArray();
        _context.RemoveRangeIfNotEmpty(interviews);

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpGet("{organizationId:guid}/interview-templates")]
    public async Task<IActionResult> OrganizationTemplatesAsync(
        [FromRoute] Guid organizationId,
        [FromQuery] PageModel pagination)
    {
        pagination ??= PageModel.Default;
        var currentUser = await _auth.CurrentUserAsync();
        if (currentUser.IsMyOrganization(organizationId) || currentUser.Has(Role.Admin))
        {
            return Ok(
                await _context.InterviewTemplates
                    .Include(x => x.Author)
                    .Where(x => x.OrganizationId == organizationId)
                    .OrderByDescending(x => x.CreatedAt)
                    .AsPaginatedAsync(x => new InterviewTemplateDto(x), pagination));
        }

        return Forbid();
    }

    [HttpGet("{organizationId:guid}/interviews")]
    public async Task<IActionResult> OrganizationInterviewsAsync(
        [FromRoute] Guid organizationId,
        [FromQuery] PageModel pagination)
    {
        pagination ??= PageModel.Default;
        var currentUser = await _auth.CurrentUserAsync();
        if (currentUser.IsMyOrganization(organizationId) || currentUser.Has(Role.Admin))
        {
            return Ok(
                await _context.Interviews
                    .Include(x => x.Interviewer)
                    .Where(x => x.OrganizationId == organizationId)
                    .OrderByDescending(x => x.CreatedAt)
                    .AsPaginatedAsync(x => new InterviewDto(x), pagination));
        }

        return Forbid();
    }

    [HttpGet("{organizationId:guid}/candidate-cards")]
    public async Task<IActionResult> CandidateCardsAsync(
        [FromRoute] Guid organizationId,
        [FromQuery] CandidateCardsFilterPaginatedRequest request)
    {
        var currentUser = await _auth.CurrentUserAsync();
        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(organizationId))
        {
            return Forbid();
        }

        var query = new OrganizationCandidateCardsQueryBuilder(
            _context,
            organizationId,
            request);

        return Ok(await query.AsPaginatedDtoAsync(request));
    }

    [HttpGet("{organizationId:guid}/candidates")]
    public async Task<IActionResult> CandidatesAsync(
        [FromRoute] Guid organizationId,
        [FromQuery] PageModel request)
    {
        var currentUser = await _auth.CurrentUserAsync();
        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(organizationId))
        {
            return Forbid();
        }

        return Ok(await _context.Candidates
            .Include(x => x.CreatedBy)
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedBy)
            .AsPaginatedAsync(x => new CandidateDto(x), request ?? PageModel.Default));
    }
}