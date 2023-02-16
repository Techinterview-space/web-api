using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Attributes;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Employments;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services.Labels;
using Domain.Services.Organizations;
using Domain.Services.Organizations.Requests;
using MG.Utils.Abstract.Extensions;
using MG.Utils.EFCore;
using MG.Utils.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TechInterviewer.Controllers.Organizations;

[HasAnyRole(Role.Interviewer)]
[ApiController]
[Route("api/candidate-cards")]
public class CandidateCardsController : ControllerBase
{
    private static readonly ICollection<EmploymentStatus> _activeStatues = new List<EmploymentStatus>
    {
        EmploymentStatus.HrInterview,
        EmploymentStatus.TechnicalInterview,
        EmploymentStatus.CustomerInterview,
        EmploymentStatus.ResourceManagerInterview,
        EmploymentStatus.DecisionPending,
        EmploymentStatus.Approved,
        EmploymentStatus.PreOffered,
        EmploymentStatus.Offered
    };

    private readonly DatabaseContext _context;
    private readonly IAuthorization _auth;

    public CandidateCardsController(DatabaseContext context, IAuthorization auth)
    {
        _context = context;
        _auth = auth;
    }

    [HttpGet("")]
    [HasAnyRole(Role.Admin)]
    public async Task<Pageable<CandidateCardDto>> AllAsync(
        [FromQuery] PageModel page)
    {
        return await _context.CandidateCards
            .Include(x => x.Candidate)
            .Include(x => x.OpenBy)
            .Include(x => x.Labels)
            .AsPaginatedAsync(x => new CandidateCardDto(x), page);
    }

    [HttpGet("organization/{organizationId:guid}")]
    public async Task<IActionResult> ForOrganizationAsync(
        [FromRoute] Guid organizationId,
        [FromQuery] CandidateCardsFilterRequest request)
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

        return Ok(await query.AsArrayDtoAsync());
    }

    [HttpGet("organization/{organizationId:guid}/available-candidates")]
    public async Task<IActionResult> AvailableCandidatesAsync(
        [FromRoute] Guid organizationId)
    {
        var currentUser = await _auth.CurrentUserAsync();
        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(organizationId))
        {
            return Forbid();
        }

        return Ok(await _context.Candidates
            .Include(x => x.CandidateCards)
            .Where(x => x.OrganizationId == organizationId)
            .Where(x => x.CandidateCards
                .Where(c => c.DeletedAt == null)
                .All(cc => !_activeStatues.Contains(cc.EmploymentStatus)))
            .AllAsync(x => new CandidateDto(x)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ByIdAsync([FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();

        var candidateCard = await _context.CandidateCards
            .Include(x => x.Candidate)
            .Include(x => x.OpenBy)
            .Include(x => x.Comments)
            .Include(x => x.Interviews)
            .ThenInclude(x => x.Interview)
            .ThenInclude(x => x.Interviewer)
            .Include(x => x.Organization)
            .Include(x => x.Labels)
            .ByIdOrFailAsync(id);

        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(candidateCard.OrganizationId))
        {
            return Forbid();
        }

        return Ok(new CandidateCardDto(candidateCard, true));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] EditCandidateCardRequest request)
    {
        var currentUser = await _auth.CurrentUserAsync();
        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(request.OrganizationId))
        {
            return Forbid();
        }

        Candidate candidate;
        if (request.CandidateId.HasValue)
        {
            candidate = await _context.Candidates
                .ByIdOrFailAsync(request.CandidateId.Value);
        }
        else
        {
            candidate = new Candidate(
                request.CandidateFirstName,
                request.CandidateLastName,
                request.CandidateContacts,
                request.OrganizationId,
                currentUser);

            await _context.Candidates.AddAsync(candidate);
        }

        var candidateCard = await _context.AddEntityAsync(new CandidateCard(
            candidate,
            currentUser,
            request.EmploymentStatus));

        candidateCard.Sync(
            await new CandidateCardLabelsCollection(
                _context,
                currentUser,
                request.Labels,
                candidateCard).PrepareAsync());

        await _context.TrySaveChangesAsync();

        return Ok(new CandidateCardDto(candidateCard, candidate));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] EditCandidateCardRequest request)
    {
        var currentUser = await _auth.CurrentUserAsync();
        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(request.OrganizationId))
        {
            return Forbid();
        }

        var card = await _context.CandidateCards
            .Include(x => x.Candidate)
            .Include(x => x.OpenBy)
            .Include(x => x.Comments)
            .Include(x => x.Interviews)
            .Include(x => x.Labels)
            .Where(x => x.OrganizationId == request.OrganizationId)
            .ByIdOrFailAsync(id);

        card.Candidate.Update(
            request.CandidateFirstName,
            request.CandidateLastName,
            request.CandidateContacts);

        card
            .Update(request.EmploymentStatus)
            .Sync(await new CandidateCardLabelsCollection(
                _context, currentUser, request.Labels, card)
                .PrepareAsync());

        await _context.TrySaveChangesAsync();
        return Ok(new CandidateCardDto(card));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatusAsync(
        [FromRoute] Guid id,
        [FromBody] EditCandidateCardEmploymentStatusRequest request)
    {
        var currentUser = await _auth.CurrentUserAsync();
        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(request.OrganizationId))
        {
            return Forbid();
        }

        var card = await _context.CandidateCards
            .Where(x => x.OrganizationId == request.OrganizationId)
            .ByIdOrFailAsync(id);

        card.Update(request.EmploymentStatus);

        await _context.TrySaveChangesAsync();
        return Ok(card.EmploymentStatus);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> ArchiveAsync(
        [FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var card = await _context.CandidateCards.ByIdOrFailAsync(id);

        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(card.OrganizationId))
        {
            return Forbid();
        }

        if (!card.Active)
        {
            return BadRequest("Cannot archive inactive card");
        }

        card.Archive();
        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> RestoreAsync(
        [FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var card = await _context.CandidateCards.ByIdOrFailAsync(id);

        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(card.OrganizationId))
        {
            return Forbid();
        }

        if (card.Active)
        {
            return BadRequest("Cannot restore active card");
        }

        card.Restore();
        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveAsync(
        [FromRoute] Guid id)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var card = await _context.CandidateCards
            .Include(x => x.Comments)
            .Include(x => x.Interviews)
            .Include(x => x.Labels)
            .ByIdOrFailAsync(id);

        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(card.OrganizationId))
        {
            return Forbid();
        }

        if (card.Active)
        {
            return BadRequest("Cannot remove active card");
        }

        _context.Remove(card);
        _context.RemoveRangeIfNotEmpty(card.Comments);
        _context.RemoveRangeIfNotEmpty(card.Interviews);

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id:guid}/comment")]
    public async Task<IActionResult> AddCommentAsync(
        [FromRoute] Guid id,
        [FromBody] AddCommentRequest request)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var card = await _context.CandidateCards
            .Include(x => x.Comments)
            .ByIdOrFailAsync(id);

        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(card.OrganizationId))
        {
            return Forbid();
        }

        if (!card.Active)
        {
            return BadRequest("Cannot add comment to inactive card");
        }

        var comment = await _context.AddEntityAsync(new CandidateCardComment(
            currentUser,
            card,
            request.Comment));

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:guid}/comment/{commentId:long}")]
    public async Task<IActionResult> DeleteCommentAsync(
        [FromRoute] Guid id,
        [FromRoute] long commentId)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var card = await _context.CandidateCards
            .Include(x => x.Comments)
            .ByIdOrFailAsync(id);

        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(card.OrganizationId))
        {
            return Forbid();
        }

        if (!card.Active)
        {
            return BadRequest("Cannot remove comments from inactive card");
        }

        var comment = card.Comments.ByIdOrFail(commentId);

        if (!comment.IsAuthor(currentUser))
        {
            return BadRequest("Cannot remove comment that is not yours");
        }

        _context.Remove(comment);
        await _context.TrySaveChangesAsync();
        return Ok();
    }
}