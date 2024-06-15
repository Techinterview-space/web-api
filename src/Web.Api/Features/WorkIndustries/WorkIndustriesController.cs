using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Labels.Models;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.WorkIndustries;

[ApiController]
[Route("api/work-industries")]
public class WorkIndustriesController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public WorkIndustriesController(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("for-select-boxes")]
    public async Task<IEnumerable<LabelEntityDto>> ForSelectBoxes(
        CancellationToken cancellationToken)
    {
        return await _context.WorkIndustries
            .Select(x => new LabelEntityDto
            {
                Id = x.Id,
                Title = x.Title,
                HexColor = x.HexColor,
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    [HttpGet("all")]
    [HasAnyRole(Role.Admin)]
    public async Task<IEnumerable<LabelEntityAdminDto>> All(
        CancellationToken cancellationToken)
    {
        return await _context.WorkIndustries
            .Select(x => new LabelEntityAdminDto
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

    [HttpPost("")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] LabelEntityEditRequest createRequest,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();

        var titleUpper = createRequest.Title?.Trim().ToUpperInvariant();
        if (await _context.WorkIndustries.AnyAsync(
                x => x.Title.ToUpper() == titleUpper,
                cancellationToken: cancellationToken))
        {
            throw new BadRequestException("Skill with this title already exists");
        }

        var item = await _context.AddEntityAsync(
            new WorkIndustry(
                createRequest.Title,
                new HexColor(createRequest.HexColor),
                currentUser),
            cancellationToken: cancellationToken);

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok(item.Id);
    }

    [HttpPut("")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> Update(
        [FromBody] LabelEntityEditRequest updateRequest,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var item = await _context.WorkIndustries.ByIdOrFailAsync(updateRequest.Id.GetValueOrDefault(), cancellationToken: cancellationToken);
        item.CouldBeUpdatedByOrFail(currentUser);

        item.Update(
            updateRequest.Title,
            new HexColor(updateRequest.HexColor));

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:long}")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> Delete(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var item = await _context.WorkIndustries.ByIdOrFailAsync(id, cancellationToken: cancellationToken);

        item.CouldBeUpdatedByOrFail(currentUser);

        _context.WorkIndustries.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok();
    }
}