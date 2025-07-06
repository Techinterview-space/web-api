using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Prompts;
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
                Type = x.Type,
                Model = x.Model,
                Engine = x.Engine,
                IsActive = x.IsActive,
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

        if (createRequest.Type is OpenAiPromptType.Undefined)
        {
            throw new BadRequestException("Id is required for prompt creation");
        }

        if (createRequest.Engine is AiEngine.Undefined)
        {
            throw new BadRequestException("Engine is required for prompt creation");
        }

        var item = await _context.SaveAsync(
            new OpenAiPrompt(
                createRequest.Type,
                createRequest.Prompt,
                createRequest.Model,
                createRequest.Engine),
            cancellationToken);

        return Ok(item.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] OpenAiPromptEditRequest updateRequest,
        CancellationToken cancellationToken)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        if (updateRequest.Type is OpenAiPromptType.Undefined)
        {
            throw new BadRequestException("Id is required for OpenAI prompt update");
        }

        var entity = await _context.OpenAiPrompts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        entity.Update(
            updateRequest.Prompt,
            updateRequest.Model,
            updateRequest.Engine);

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        [FromRoute(Name = "id")] Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await _context.OpenAiPrompts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        entity.Activate();

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        [FromRoute(Name = "id")] Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await _context.OpenAiPrompts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        entity.Deactivate();

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute(Name = "id")] Guid id,
        CancellationToken cancellationToken)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        var entity = await _context.OpenAiPrompts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        _context.Remove(entity);
        await _context.TrySaveChangesAsync(cancellationToken);

        return Ok();
    }
}