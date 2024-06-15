using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Interviews;
using Domain.Enums;
using Domain.Validation;
using Domain.ValueObjects.Pagination;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Interviews.Models;
using Web.Api.Features.Labels.Models;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Interviews;

[ApiController]
[Route("api/interview-templates")]
public class InterviewTemplateController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public InterviewTemplateController(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet]
    [HasAnyRole(Role.Admin)]
    public async Task<Pageable<InterviewTemplateDto>> AllAsync([FromQuery] PageModel pageParams = null)
    {
        pageParams ??= PageModel.Default;
        return await _context.InterviewTemplates
            .Include(x => x.Author)
            .Include(x => x.Labels)
            .OrderByDescending(x => x.CreatedAt)
            .AsPaginatedAsync(x => new InterviewTemplateDto(x), pageParams);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<Pageable<InterviewTemplateDto>> PublicAsync([FromQuery] PageModel pageParams = null)
    {
        pageParams ??= PageModel.Default;
        return await _context.InterviewTemplates
            .Include(x => x.Author)
            .Include(x => x.Labels)
            .Where(x => x.IsPublic)
            .OrderByDescending(x => x.CreatedAt)
            .AsPaginatedAsync(x => new InterviewTemplateDto(x), pageParams);
    }

    [HttpGet("available-for-interview")]
    [HasAnyRole]
    public async Task<IEnumerable<InterviewTemplateDto>> AvailableForInterviewAsync()
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();

        return await _context.InterviewTemplates
            .Include(x => x.Author)
            .Include(x => x.Labels)
            .Where(x => x.IsPublic || x.AuthorId == currentUser.Id)
            .OrderByDescending(x => x.CreatedAt)
            .AllAsync(x => new InterviewTemplateDto(x));
    }

    [HttpGet("my")]
    [HasAnyRole]
    public async Task<IEnumerable<InterviewTemplateDto>> MyTemplatesAsync()
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();

        return await _context.InterviewTemplates
            .Include(x => x.Labels)
            .Where(x => x.AuthorId == currentUser.Id)
            .OrderByDescending(x => x.CreatedAt)
            .AllAsync(x => new InterviewTemplateDto(x));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> ByIdAsync(Guid id)
    {
        var template = await _context.InterviewTemplates
            .Include(x => x.Labels)
            .Include(x => x.Author)
            .AsNoTracking()
            .ByIdOrFailAsync(id);

        if (template.IsPublic)
        {
            return Ok(new InterviewTemplateDto(template));
        }

        var currentUser = await _auth.CurrentUserOrNullAsync();
        if (template.CouldBeOpenBy(currentUser))
        {
            return Ok(new InterviewTemplateDto(template));
        }

        return StatusCode(StatusCodes.Status403Forbidden);
    }

    [HttpPost]
    [HasAnyRole]
    public async Task<IActionResult> CreateAsync([FromBody] InterviewTemplateCreateRequest createRequest)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();

        var interviewTemplate = await _context.AddEntityAsync(new InterviewTemplate(
            createRequest.Title,
            createRequest.OverallOpinion,
            createRequest.IsPublic,
            createRequest.Subjects,
            currentUser));

        interviewTemplate.Sync(await new UserLabelsCollection(_context, currentUser, createRequest.Labels).PrepareAsync());

        interviewTemplate.ThrowIfInvalid();
        await _context.TrySaveChangesAsync();
        return Ok(interviewTemplate.Id);
    }

    [HttpPut]
    [HasAnyRole]
    public async Task<IActionResult> UpdateAsync([FromBody] InterviewTemplateUpdateRequest updateRequest)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();
        var interviewTemplate = await _context.InterviewTemplates
            .Include(x => x.Labels)
            .ByIdOrFailAsync(updateRequest.Id);

        if (!interviewTemplate.CouldBeEditBy(currentUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        interviewTemplate
            .Update(
                updateRequest.Title,
                updateRequest.OverallOpinion,
                updateRequest.IsPublic,
                updateRequest.Subjects)
            .Sync(await new UserLabelsCollection(_context, currentUser, updateRequest.Labels).PrepareAsync())
            .ThrowIfInvalid();

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [HasAnyRole]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var interviewTemplate = await _context.InterviewTemplates.ByIdOrFailAsync(id);
        if (!interviewTemplate.CouldBeEditBy(await _auth.CurrentUserOrNullAsync()))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        _context.InterviewTemplates.Remove(interviewTemplate);
        await _context.TrySaveChangesAsync();
        return Ok();
    }
}