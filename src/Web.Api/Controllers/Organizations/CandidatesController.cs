using System;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services.Organizations;
using Domain.ValueObjects.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Controllers.Organizations;

[HasAnyRole(Role.Interviewer)]
[ApiController]
[Route("api/candidates")]
public class CandidatesController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _auth;

    public CandidatesController(DatabaseContext context, IAuthorization auth)
    {
        _context = context;
        _auth = auth;
    }

    [HttpGet("")]
    [HasAnyRole(Role.Admin)]
    public async Task<Pageable<CandidateDto>> AllAsync(
        [FromQuery] PageModel page)
    {
        return await _context.Candidates
            .Include(x => x.Organization)
            .Include(x => x.CreatedBy)
            .AsPaginatedAsync(x => new CandidateDto(x), page);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ByIdAsync(
        [FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();

        var candidate = await _context.Candidates
            .Include(x => x.Organization)
            .Include(x => x.CreatedBy)
            .ByIdOrFailAsync(id);

        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(candidate.OrganizationId))
        {
            return Forbid();
        }

        return Ok(new CandidateDto(candidate));
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> ArchiveAsync(
        [FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();

        var candidate = await _context.Candidates
            .ByIdOrFailAsync(id);

        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(candidate.OrganizationId))
        {
            return Forbid();
        }

        if (!candidate.Active)
        {
            return BadRequest("Candidate is already archived.");
        }

        candidate.Archive();
        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> RestoreAsync(
        [FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();

        var candidate = await _context.Candidates
            .ByIdOrFailAsync(id);

        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(candidate.OrganizationId))
        {
            return Forbid();
        }

        if (candidate.Active)
        {
            return BadRequest("Candidate is already activated.");
        }

        candidate.Restore();
        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();

        var candidate = await _context.Candidates
            .ByIdOrFailAsync(id);

        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(candidate.OrganizationId))
        {
            return Forbid();
        }

        if (candidate.Active)
        {
            return BadRequest("The candidate should be archived first");
        }

        _context.Remove(candidate);
        await _context.TrySaveChangesAsync();
        return Ok();
    }
}