using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Labels;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Services.Labels;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Controllers.Skills.Dtos;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Controllers.Skills;

[ApiController]
[Route("api/skills")]
[HasAnyRole]
public class SkillsController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public SkillsController(IAuthorization auth, DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("all")]
    [HasAnyRole(Role.Admin)]
    public async Task<IEnumerable<SkillAdminDto>> All(
        CancellationToken cancellationToken)
    {
        return await _context.Skills
            .Select(x => new SkillAdminDto
            {
                Id = x.Id,
                Title = x.Title,
                HexColor = x.HexColor,
                CreatedById = x.CreatedById,
                CreatedBy = x.CreatedBy.Email,
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    [HttpGet("for-form")]
    public async Task<IEnumerable<SkillDto>> ForForm(
        CancellationToken cancellationToken)
    {
        return await _context.Skills
            .Select(x => new SkillDto
            {
                Id = x.Id,
                Title = x.Title,
                HexColor = x.HexColor,
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(
        [FromBody] SkillEditRequest createRequest,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();

        var titleUpper = createRequest.Title?.Trim().ToUpperInvariant();
        if (await _context.Skills.AnyAsync(
                x => x.CreatedById == currentUser.Id && x.Title.ToUpperInvariant() == titleUpper,
                cancellationToken: cancellationToken))
        {
            throw new BadRequestException("Skill with this title already exists");
        }

        var label = await _context.AddEntityAsync(
            new Skill(
                createRequest.Title,
                new HexColor(createRequest.HexColor),
                currentUser),
            cancellationToken: cancellationToken);

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok(label.Id);
    }

    [HttpPut("")]
    public async Task<IActionResult> Update(
        [FromBody] SkillEditRequest updateRequest,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();
        var skill = await _context.Skills.ByIdOrFailAsync(updateRequest.Id.GetValueOrDefault(), cancellationToken: cancellationToken);
        skill.CouldBeUpdatedByOrFail(currentUser);

        skill.Update(
            updateRequest.Title,
            new HexColor(updateRequest.HexColor));

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();
        var skill = await _context.Skills.ByIdOrFailAsync(id, cancellationToken: cancellationToken);

        skill.CouldBeUpdatedByOrFail(currentUser);

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok();
    }
}