using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Attributes;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Labels;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services.Labels;
using MG.Utils.EFCore;
using MG.Utils.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TechInterviewer.Controllers.Labels;

[ApiController]
[Route("api/organization-labels")]
[HasAnyRole]
public class OrganizationLabelsController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public OrganizationLabelsController(IAuthorization auth, DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> OrgLabelsAsync([FromRoute] Guid organizationId)
    {
        var currentUser = await _auth.CurrentUserAsync();
        if (!currentUser.Has(Role.Admin) && !currentUser.IsMyOrganization(organizationId))
        {
            return Forbid();
        }

        return Ok(await _context.OrganizationLabels
            .Where(x => x.OrganizationId == organizationId)
            .AllAsync(x => new LabelDto(x)));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateAsync([FromBody] LabelDto createRequest)
    {
        var currentUser = await _auth.CurrentUserAsync();
        if (!createRequest.OrganizationId.HasValue)
        {
            return BadRequest("OrganizationId is required");
        }

        if (!currentUser.IsMyOrganization(createRequest.OrganizationId.Value))
        {
            return BadRequest("You are not authorized to create labels for this organization");
        }

        var titleUpper = createRequest.Title.ToUpperInvariant();
        if (await _context.OrganizationLabels.AnyAsync(x =>
                x.OrganizationId == createRequest.OrganizationId.Value &&
                x.Title.ToUpper() == titleUpper))
        {
            return BadRequest("Label with this title already exists");
        }

        var label = await _context.AddEntityAsync(new OrganizationLabel(
            createRequest.Title,
            createRequest.OrganizationId.Value,
            new HexColor(createRequest.HexColor),
            currentUser));

        await _context.TrySaveChangesAsync();
        return Ok(label.Id);
    }

    [HttpPut("")]
    public async Task<IActionResult> UpdateAsync([FromBody] LabelDto updateRequest)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var label = await _context.OrganizationLabels.ByIdOrFailAsync(updateRequest.Id.GetValueOrDefault());
        label.CouldBeUpdatedByOrFail(currentUser);

        label.Update(
            updateRequest.Title,
            new HexColor(updateRequest.HexColor));

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var label = await _context.OrganizationLabels
            .Include(x => x.Organization)
            .ByIdOrFailAsync(id);

        if (!label.CouldBeDeletedBy(currentUser))
        {
            return Forbid();
        }

        _context.OrganizationLabels.Remove(label);
        await _context.TrySaveChangesAsync();

        return Ok();
    }
}