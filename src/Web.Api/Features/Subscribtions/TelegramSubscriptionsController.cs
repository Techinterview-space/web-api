using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Subscribtions.ActivateSubscription;
using Web.Api.Features.Subscribtions.CreateSubscription;
using Web.Api.Features.Subscribtions.DeactivateSubscription;
using Web.Api.Features.Subscribtions.DeleteSubscription;
using Web.Api.Features.Subscribtions.EditSubscription;
using Web.Api.Features.Subscribtions.GetOpenAiReport;
using Web.Api.Features.Subscribtions.GetOpenAiReportAnalysis;
using Web.Api.Features.Subscribtions.GetStatDataChangeSubscriptions;
using Web.Api.Features.Subscribtions.SendUpdatesToSubscriptionChat;
using Web.Api.Features.Subscribtions.Shared;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Subscribtions;

[ApiController]
[Route("api/telegram-subscriptions")]
[HasAnyRole(Role.Admin)]
public class TelegramSubscriptionsController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public TelegramSubscriptionsController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("")]
    public async Task<Pageable<StatDataChangeSubscriptionDto>> Search(
        [FromQuery] GetStatDataChangeSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetStatDataChangeSubscriptionsHandler, GetStatDataChangeSubscriptionsQuery, Pageable<StatDataChangeSubscriptionDto>>(
            request,
            cancellationToken);
    }

    [HttpPost("")]
    public async Task<StatDataChangeSubscriptionDto> Create(
        [FromBody] CreateSubscriptionBodyRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<CreateSubscriptionHandler, CreateSubscriptionCommand, StatDataChangeSubscriptionDto>(
            new CreateSubscriptionCommand(
                request),
            cancellationToken);
    }

    [HttpPost("{id:guid}")]
    public async Task<StatDataChangeSubscriptionDto> Update(
        [FromRoute] Guid id,
        [FromBody] EditSubscriptionBodyRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<EditSubscriptionHandler, EditSubscriptionCommand, StatDataChangeSubscriptionDto>(
            new EditSubscriptionCommand(
                id,
                request),
            cancellationToken);
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<ActivateStatDataChangeSubscriptionHandler, ActivateStatDataChangeSubscriptionCommand, Nothing>(
            new ActivateStatDataChangeSubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeactivateStatDataChangeSubscriptionHandler, DeactivateStatDataChangeSubscriptionCommand, Nothing>(
            new DeactivateStatDataChangeSubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeleteSubscriptionHandler, DeleteSubscriptionCommand, Nothing>(
            new DeleteSubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/open-ai-analysis")]
    public async Task<IActionResult> GetOpenAiAnalysis(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<GetOpenAiReportAnalysisHandler, GetOpenAiReportAnalysisQuery, GetOpenAiReportAnalysisResponse>(
                new GetOpenAiReportAnalysisQuery(id),
                cancellationToken));
    }

    [HttpGet("{id:guid}/open-ai-report")]
    public async Task<IActionResult> GetOpenAiReport(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<GetOpenAiReportHandler, GetOpenAiReportQuery, OpenAiBodyReport>(
                new GetOpenAiReportQuery(id),
                cancellationToken));
    }

    [HttpPost("{id:guid}/send-updates")]
    public async Task<IActionResult> SendUpdates(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<SendUpdatesToSubscriptionChatHandler, SendUpdatesToSubscriptionChatCommand, int>(
            new SendUpdatesToSubscriptionChatCommand(id),
            cancellationToken);

        return NoContent();
    }
}