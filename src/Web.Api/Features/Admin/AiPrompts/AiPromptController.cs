using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.OpenAI;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Admin.AiPrompts.Models;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Admin.AiPrompts;

[HasAnyRole(Role.Admin)]
[ApiController]
[Route("api/admin/ai-prompts")]
public class AiPromptController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public AiPromptController(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("")]
    public async Task<List<OpenAiPromptDto>> All(
        CancellationToken cancellationToken)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        return await _context.OpenAiPrompts
            .OrderBy(x => x.Id)
            .Select(x => new OpenAiPromptDto
            {
                Id = x.Id,
                Prompt = x.Prompt,
                Model = x.Model,
                Engine = x.Engine,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(
        [FromBody] OpenAiPromptEditRequest createRequest,
        CancellationToken cancellationToken)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        if (!createRequest.Id.HasValue)
        {
            throw new BadRequestException("Id is required for OpenAI prompt creation");
        }

        if (await _context.OpenAiPrompts.AnyAsync(
                x => x.Id == createRequest.Id.Value,
                cancellationToken: cancellationToken))
        {
            throw new BadRequestException("OpenAI prompt with this ID already exists");
        }

        var item = await _context.SaveAsync(
            new OpenAiPrompt(
                createRequest.Id.Value,
                createRequest.Prompt,
                createRequest.Model,
                createRequest.Engine),
            cancellationToken);
        return Ok(item.Id);
    }

    [HttpPut("")]
    public async Task<IActionResult> Update(
        [FromBody] OpenAiPromptEditRequest updateRequest,
        CancellationToken cancellationToken)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        if (!updateRequest.Id.HasValue)
        {
            throw new BadRequestException("Id is required for OpenAI prompt update");
        }

        var entity = await _context.OpenAiPrompts
            .ByIdOrFailAsync(updateRequest.Id.Value, cancellationToken);

        // Update entity properties
        entity.UpdatePrompt(updateRequest.Prompt);
        entity.UpdateModel(updateRequest.Model);
        entity.UpdateEngine(updateRequest.Engine);

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        [FromRoute] OpenAiPromptType id,
        CancellationToken cancellationToken)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        var entity = await _context.OpenAiPrompts
            .ByIdOrFailAsync(id, cancellationToken);

        _context.Remove(entity);
        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }
}