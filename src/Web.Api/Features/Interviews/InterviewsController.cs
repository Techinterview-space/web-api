using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Interviews;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.PDF.Interviews;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Features.Interviews.Models;
using TechInterviewer.Features.Interviews.RevokeShareLinkToken;
using TechInterviewer.Features.Labels.Models;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Features.Interviews;

[ApiController]
[Route("api/interviews")]
[HasAnyRole]
public class InterviewsController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;
    private readonly IInterviewPdfService _pdf;

    public InterviewsController(
        IAuthorization auth,
        DatabaseContext context,
        IInterviewPdfService pdf)
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ByIdAsync(Guid id)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var interview = await _context.Interviews
            .Include(x => x.Interviewer)
            .Include(x => x.Labels)
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
        var currentUser = await _auth.CurrentUserOrFailAsync(cancellationToken);
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
        var currentUser = await _auth.CurrentUserOrFailAsync(cancellationToken);
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
        var currentUser = await _auth.CurrentUserOrFailAsync(cancellationToken);
        var interview = await _context.Interviews
            .Include(x => x.Interviewer)
            .Include(x => x.Labels)
            .ByIdOrFailAsync(id, cancellationToken);

        CheckPermissions(interview, currentUser);

        var file = _pdf.Render(interview);
        return File(file.Data, file.ContentType, file.Filename);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] InterviewCreateRequest createRequest,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync(cancellationToken);

        var interview = await _context.AddEntityAsync(
            new Interview(
                createRequest.CandidateName,
                createRequest.OverallOpinion,
                createRequest.CandidateGrade,
                createRequest.Subjects,
                currentUser),
            cancellationToken);

        interview.Sync(
            await new UserLabelsCollection(_context, currentUser, createRequest.Labels)
                .PrepareAsync(cancellationToken));
        interview.ThrowIfInvalid();

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok(interview.Id);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync(
        [FromBody] InterviewUpdateRequest updateRequest,
        CancellationToken cancellationToken)
    {
        var interview = await _context.Interviews
            .Include(x => x.Labels)
            .ByIdOrFailAsync(updateRequest.Id, cancellationToken);

        var currentUser = await _auth.CurrentUserOrFailAsync(cancellationToken);
        CheckPermissions(interview, currentUser);

        interview
            .Update(
                updateRequest.CandidateName,
                updateRequest.OverallOpinion,
                updateRequest.CandidateGrade,
                updateRequest.Subjects)
            .Sync(
                await new UserLabelsCollection(_context, currentUser, updateRequest.Labels)
                    .PrepareAsync(cancellationToken))
            .ThrowIfInvalid();

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var interview = await _context.Interviews
            .Include(x => x.Labels)
            .ByIdOrFailAsync(id, cancellationToken);

        CheckPermissions(interview, await _auth.CurrentUserOrFailAsync(cancellationToken));

        _context.Interviews.Remove(interview);

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPost("{id:guid}/get-share-token")]
    public async Task<GetShareLinkTokenResult> GetShareToken(
        Guid id,
        CancellationToken cancellationToken)
    {
        var interview = await _context.Interviews
            .Include(x => x.ShareLink)
            .ByIdOrFailAsync(id, cancellationToken);

        if (interview.ShareLink == null)
        {
            var shareLink = await _context.SaveAsync(new ShareLink(interview), cancellationToken);
            return new GetShareLinkTokenResult(shareLink);
        }

        return new GetShareLinkTokenResult(interview.ShareLink);
    }

    [HttpPost("{id:guid}/revoke-share-link")]
    public async Task<GetShareLinkTokenResult> RevokeShareLink(
        Guid id,
        CancellationToken cancellationToken)
    {
        var interview = await _context.Interviews
            .Include(x => x.ShareLink)
            .ByIdOrFailAsync(id, cancellationToken);

        if (interview.ShareLink == null)
        {
            var shareLink = await _context.SaveAsync(new ShareLink(interview), cancellationToken);
            return new GetShareLinkTokenResult(shareLink);
        }

        interview.ShareLink.RevokeToken();
        await _context.TrySaveChangesAsync(cancellationToken);

        return new GetShareLinkTokenResult(interview.ShareLink);
    }

    [HttpGet("{id:guid}/with-token/{secret_token:guid}")]
    public async Task<IActionResult> GetInterviewByShareToken(
        [FromRoute(Name = "id")] Guid interviewId,
        [FromRoute(Name = "secret_token")] Guid shareToken,
        CancellationToken cancellationToken)
    {
        var interview = await _context.Interviews
            .Include(x => x.ShareLink)
            .Include(x => x.Labels)
            .Where(i => i.Id == interviewId && i.ShareLink.ShareToken == shareToken)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (interview == null)
        {
            throw new NotFoundException($"Did not find any {nameof(Interview)} by shareToken {shareToken}");
        }

        return Ok(new InterviewDto(interview));
    }

    private static void CheckPermissions(Interview interviewTemplate, User currentUser)
    {
        if (!interviewTemplate.CouldBeOpenBy(currentUser) && !currentUser.Has(Role.Admin))
        {
            throw new NoPermissionsException();
        }
    }
}