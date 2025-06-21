using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Web.Api.Features.Github.DeleteGithubProcessingJob;
using Web.Api.Features.Github.GetGithubProcessingJobs;
using Web.Api.Features.Github.GetGithubProfileChats;
using Web.Api.Features.Github.GetGithubProfiles;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Github;

[ApiController]
[Route("api/github")]
[HasAnyRole(Role.Admin)]
public class GithubController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public GithubController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("profiles")]
    public async Task<IActionResult> GetGithubProfiles(
        [FromQuery] GetGithubProfilesQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.GetRequiredService<GetGithubProfilesHandler>()
                .Handle(queryParams, cancellationToken));
    }

    [HttpGet("chats")]
    public async Task<IActionResult> GetGithubProfileChats(
        [FromQuery] GetGithubProfileChatsQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.GetRequiredService<GetGithubProfileChatsHandler>()
                .Handle(queryParams, cancellationToken));
    }

    [HttpGet("processing-jobs")]
    public async Task<IActionResult> GetGithubProcessingJobs(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.GetRequiredService<GetGithubProcessingJobsHandler>()
                .Handle(Nothing.Value, cancellationToken));
    }

    [HttpDelete("processing-jobs/{username}")]
    public async Task<IActionResult> DeleteGithubProcessingJob(
        string username,
        CancellationToken cancellationToken)
    {
        try
        {
            await _serviceProvider.GetRequiredService<DeleteGithubProcessingJobHandler>()
                .Handle(new DeleteGithubProcessingJobCommand(username), cancellationToken);

            return Ok();
        }
        catch (BadHttpRequestException ex)
        {
            return NotFound(ex.Message);
        }
    }
}