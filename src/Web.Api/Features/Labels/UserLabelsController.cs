using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Labels;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Labels.Models;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Labels;

[ApiController]
[Route("api/user-labels")]
[HasAnyRole]
public class UserLabelsController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public UserLabelsController(IAuthorization auth, DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("my")]
    public async Task<IEnumerable<LabelDto>> MyLabelsAsync()
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        return await _context.UserLabels
            .Where(x => x.CreatedById == currentUser.Id)
            .AllAsync(x => new LabelDto(x));
    }

    [HttpGet("empty")]
    [HasAnyRole(Role.Admin)]
    public async Task<IEnumerable<LabelDto>> EmptyAsync()
    {
        return await _context.UserLabels
            .Where(x => x.CreatedById == null)
            .AllAsync(x => new LabelDto(x));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateAsync([FromBody] LabelDto createRequest)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();

        var titleUpper = createRequest.Title.ToUpperInvariant();
        if (await _context.UserLabels.AnyAsync(x =>
                x.CreatedById == currentUser.Id && x.Title.ToUpper() == titleUpper))
        {
            throw new BadRequestException("Label with this title already exists");
        }

        var label = await _context.AddEntityAsync(new UserLabel(
            createRequest.Title,
            new HexColor(createRequest.HexColor),
            currentUser));

        await _context.TrySaveChangesAsync();
        return Ok(label.Id);
    }

    [HttpPut("")]
    public async Task<IActionResult> UpdateAsync([FromBody] LabelDto updateRequest)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var label = await _context.UserLabels.ByIdOrFailAsync(updateRequest.Id.GetValueOrDefault());
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
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var label = await _context.UserLabels.ByIdOrFailAsync(id);

        label.CouldBeUpdatedByOrFail(currentUser);

        _context.UserLabels.Remove(label);
        await _context.SaveChangesAsync();

        return Ok();
    }
}