using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Employments;
using Domain.Entities.Interviews;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Services.Interviews;
using Domain.Services.Interviews.Dtos;
using Domain.Services.Labels;
using Domain.Validation;
using Domain.ValueObjects.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Controllers;

[ApiController]
[Route("api/interviews")]
[HasAnyRole]
public class InterviewsController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;
    private readonly IInterviewPdf _pdf;

    public InterviewsController(IAuthorization auth, DatabaseContext context, IInterviewPdf pdf)
    {
        _auth = auth;
        _context = context;
        _pdf = pdf;
    }

    [HttpGet]
    [HasAnyRole(Role.Admin)]
    public async Task<IEnumerable<InterviewDto>> AllAsync()
    {
        return await _context.Interviews
            .Include(x => x.Interviewer)
            .Include(x => x.Labels)
            .OrderByDescending(x => x.CreatedAt)
            .AllAsync(x => new InterviewDto(x));
    }

    [HttpGet("my")]
    public async Task<IEnumerable<InterviewDto>> MyInterviewsAsync()
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        return await _context.Interviews
            .Include(x => x.Labels)
            .Where(x => x.InterviewerId == currentUser.Id)
            .OrderByDescending(x => x.CreatedAt)
            .AllAsync(x => new InterviewDto(x));
    }

    [HttpGet("organization/{organizationId:guid}")]
    public async Task<IActionResult> OrganizationInterviewsAsync(
        [FromRoute] Guid organizationId,
        [FromQuery] PageModel pagination)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        if (currentUser.Has(Role.Admin) ||
            currentUser.IsMyOrganization(organizationId))
        {
            return Ok(await _context.Interviews
                .Include(x => x.Labels)
                .Where(x => x.OrganizationId == organizationId)
                .OrderByDescending(x => x.CreatedAt)
                .AsPaginatedAsync(x => new InterviewDto(x), pagination ?? PageModel.Default));
        }

        return StatusCode(StatusCodes.Status403Forbidden);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ByIdAsync(Guid id)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var interview = await _context.Interviews
            .Include(x => x.Interviewer)
            .Include(x => x.Labels)
            .Include(x => x.Organization)
            .Include(x => x.CandidateInterview)
            .ThenInclude(x => x.CandidateCard)
            .ThenInclude(x => x.Candidate)
            .ByIdOrFailAsync(id);

        if (interview.CouldBeOpenBy(currentUser) ||
            currentUser.Has(Role.Admin))
        {
            return Ok(new InterviewDto(interview));
        }

        return StatusCode(StatusCodes.Status403Forbidden);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<FileContentResult> PdfAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var interview = await _context.Interviews
            .Include(x => x.Interviewer)
            .Include(x => x.Labels)
            .ByIdOrFailAsync(id, cancellationToken: cancellationToken);

        CheckPermissions(interview, currentUser);

        var file = await _pdf.RenderAsync(interview, cancellationToken);
        return File(file.Data, file.ContentType, file.Filename);
    }

    [HttpGet("{id:guid}/markdown")]
    public async Task<IActionResult> MarkdownAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var interview = await _context.Interviews
            .Include(x => x.Interviewer)
            .ByIdOrFailAsync(id, cancellationToken: cancellationToken);

        CheckPermissions(interview, currentUser);

        return Ok(new
        {
            Markdown = new InterviewMarkdown(interview).ToString()
        });
    }

    // TODO Remove
    [HttpGet("{id:guid}/download-sync")]
    public async Task<FileContentResult> DownloadSyncAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var interview = await _context.Interviews
            .Include(x => x.Interviewer)
            .Include(x => x.Labels)
            .ByIdOrFailAsync(id, cancellationToken);

        CheckPermissions(interview, currentUser);

        var file = _pdf.Render(interview);
        return File(file.Data, file.ContentType, file.Filename);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] InterviewCreateRequest createRequest)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();

        var interview = await _context.AddEntityAsync(new Interview(
            createRequest.CandidateName,
            createRequest.OverallOpinion,
            createRequest.CandidateGrade,
            createRequest.Subjects,
            currentUser));

        interview.Sync(await new UserLabelsCollection(_context, currentUser, createRequest.Labels).PrepareAsync());
        interview.ThrowIfInvalid();

        await _context.TrySaveChangesAsync();
        return Ok(interview.Id);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] InterviewUpdateRequest updateRequest)
    {
        var interview = await _context.Interviews
            .Include(x => x.Labels)
            .Include(x => x.CandidateInterview)
            .ByIdOrFailAsync(updateRequest.Id);

        var currentUser = await _auth.CurrentUserOrFailAsync();
        CheckPermissions(interview, currentUser);

        interview
            .Update(
                updateRequest.CandidateName,
                updateRequest.OverallOpinion,
                updateRequest.CandidateGrade,
                updateRequest.Subjects)
            .Sync(await new UserLabelsCollection(_context, currentUser, updateRequest.Labels).PrepareAsync())
            .ThrowIfInvalid();

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var interview = await _context.Interviews
            .Include(x => x.Labels)
            .Include(x => x.CandidateInterview)
            .ByIdOrFailAsync(id);

        CheckPermissions(interview, await _auth.CurrentUserOrFailAsync());

        _context.Interviews.Remove(interview);

        if (interview.CandidateInterview is not null)
        {
            _context.Remove(interview.CandidateInterview);
        }

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    private static void CheckPermissions(Interview interviewTemplate, User currentUser)
    {
        if (!interviewTemplate.CouldBeOpenBy(currentUser) && !currentUser.Has(Role.Admin))
        {
            throw new NoPermissionsException();
        }
    }
}