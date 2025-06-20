using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Github;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Github.Dtos;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Github;

[ApiController]
[Route("api/github")]
[HasAnyRole(Role.Admin)]
public class GithubController : ControllerBase
{
    private readonly DatabaseContext _context;

    public GithubController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet("profiles")]
    public async Task<IActionResult> GetGithubProfiles(
        [FromQuery] PageModel queryParams,
        CancellationToken cancellationToken)
    {
        var profiles = await _context.GithubProfiles
            .OrderByDescending(x => x.CreatedAt)
            .AsPaginatedAsync(
                queryParams,
                cancellationToken);

        return Ok(new Pageable<GithubProfileDto>(
            profiles.CurrentPage,
            profiles.PageSize,
            profiles.TotalItems,
            profiles.Results
                .Select(x => new GithubProfileDto(x))
                .ToList()));
    }

    [HttpGet("chats")]
    public async Task<IActionResult> GetGithubProfileChats(
        [FromQuery] PageModel queryParams,
        CancellationToken cancellationToken)
    {
        var chats = await _context.GithubProfileBotChats
            .OrderByDescending(x => x.MessagesCount)
            .ThenByDescending(x => x.CreatedAt)
            .AsPaginatedAsync(
                queryParams,
                cancellationToken);

        return Ok(new Pageable<GithubProfileBotChatDto>(
            chats.CurrentPage,
            chats.PageSize,
            chats.TotalItems,
            chats.Results
                .Select(x => new GithubProfileBotChatDto(x))
                .ToList()));
    }

    [HttpGet("processing-jobs")]
    public async Task<IActionResult> GetGithubProcessingJobs(
        CancellationToken cancellationToken)
    {
        var jobs = await _context.GithubProfileProcessingJobs
            .OrderByDescending(x => x.CreatedAt)
            .AllAsync(cancellationToken);

        return Ok(jobs.Select(x => new GithubProfileProcessingJobDto(x)).ToList());
    }

    [HttpDelete("processing-jobs/{username}")]
    public async Task<IActionResult> DeleteGithubProcessingJob(
        string username,
        CancellationToken cancellationToken)
    {
        var job = await _context.GithubProfileProcessingJobs
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (job == null)
        {
            return NotFound($"Github processing job with username '{username}' not found");
        }

        _context.GithubProfileProcessingJobs.Remove(job);
        await _context.TrySaveChangesAsync(cancellationToken);

        return Ok();
    }
}