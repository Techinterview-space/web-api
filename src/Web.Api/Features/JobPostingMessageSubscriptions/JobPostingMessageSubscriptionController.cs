using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData.Salary;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.JobPostingMessageSubscriptions.Models;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.JobPostingMessageSubscriptions;

[ApiController]
[Route("api/job-posting-message-subscriptions")]
[HasAnyRole(Role.Admin)]
public class JobPostingMessageSubscriptionController : ControllerBase
{
    private readonly DatabaseContext _context;

    public JobPostingMessageSubscriptionController(
        DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<Pageable<JobPostingMessageSubscriptionDto>> Search(
        [FromQuery] PageModel pageModel,
        CancellationToken cancellationToken)
    {
        return await _context.JobPostingMessageSubscriptions
            .OrderBy(x => x.CreatedAt)
            .Select(JobPostingMessageSubscriptionDto.Transform)
            .AsPaginatedAsync(pageModel, cancellationToken);
    }

    [HttpPost("")]
    public async Task<JobPostingMessageSubscriptionDto> Create(
        [FromBody] CreateJobPostingMessageSubscriptionBody request,
        CancellationToken cancellationToken)
    {
        if (request.ProfessionIds == null || request.ProfessionIds.Count == 0)
        {
            throw new BadRequestException("At least one profession ID must be provided.");
        }

        var hasSubscriptionForThisTelegramChatId = await _context.JobPostingMessageSubscriptions
            .AnyAsync(x => x.TelegramChatId == request.TelegramChatId, cancellationToken);

        if (hasSubscriptionForThisTelegramChatId)
        {
            throw new BadRequestException("A subscription for this Telegram chat ID already exists.");
        }

        var subscription = new JobPostingMessageSubscription(
            request.Name,
            request.TelegramChatId,
            request.ProfessionIds);

        _context.JobPostingMessageSubscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        return new JobPostingMessageSubscriptionDto(subscription);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var subscription = await _context.JobPostingMessageSubscriptions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Subscription not found.");

        subscription.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var subscription = await _context.JobPostingMessageSubscriptions
                               .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                           ?? throw new NotFoundException("Subscription not found.");

        subscription.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var subscription = await _context.JobPostingMessageSubscriptions
                               .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                           ?? throw new NotFoundException("Subscription not found.");

        _context.JobPostingMessageSubscriptions.Remove(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}