using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.AiServices.Custom.Models;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.SalarySubscribtions.ActivateSubscription;
using Web.Api.Features.SalarySubscribtions.CreateSubscription;
using Web.Api.Features.SalarySubscribtions.DeactivateSubscription;
using Web.Api.Features.SalarySubscribtions.DeleteSubscription;
using Web.Api.Features.SalarySubscribtions.EditSubscription;
using Web.Api.Features.SalarySubscribtions.GetOpenAiReportAnalysis;
using Web.Api.Features.SalarySubscribtions.GetSalaryAiReport;
using Web.Api.Features.SalarySubscribtions.GetSalarySubscriptions;
using Web.Api.Features.SalarySubscribtions.SendUpdatesToSubscriptionChat;
using Web.Api.Features.SalarySubscribtions.Shared;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.SalarySubscribtions;

[ApiController]
[Route("api/telegram-subscriptions")]
[HasAnyRole(Role.Admin)]
public class SalaryTelegramSubscriptionsController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public SalaryTelegramSubscriptionsController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("")]
    public async Task<Pageable<SalarySubscriptionDto>> Search(
        [FromQuery] GetSalarySubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetSalarySubscriptionsHandler, GetSalarySubscriptionsQuery, Pageable<SalarySubscriptionDto>>(
            request,
            cancellationToken);
    }

    [HttpPost("")]
    public async Task<SalarySubscriptionDto> Create(
        [FromBody] CreateSalarySubscriptionBodyRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<CreateSubscriptionHandler, CreateSalarySubscriptionCommand, SalarySubscriptionDto>(
            new CreateSalarySubscriptionCommand(
                request),
            cancellationToken);
    }

    [HttpPost("{id:guid}")]
    public async Task<SalarySubscriptionDto> Update(
        [FromRoute] Guid id,
        [FromBody] EditSalarySubscriptionBodyRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<EditSalarySubscriptionHandler, EditSalarySubscriptionCommand, SalarySubscriptionDto>(
            new EditSalarySubscriptionCommand(
                id,
                request),
            cancellationToken);
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<ActivateSalarySubscriptionHandler, ActivateSalarySubscriptionCommand, Nothing>(
            new ActivateSalarySubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeactivateSalarySubscriptionHandler, DeactivateSalarySubscriptionCommand, Nothing>(
            new DeactivateSalarySubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeleteSalarySubscriptionHandler, DeleteSalarySubscriptionCommand, Nothing>(
            new DeleteSalarySubscriptionCommand(id),
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
            await _serviceProvider.HandleBy<GetSalaryAiReportHandler, GetSalaryAiReportQuery, OpenAiBodyReport>(
                new GetSalaryAiReportQuery(id),
                cancellationToken));
    }

    [HttpPost("{id:guid}/send-updates")]
    public async Task<IActionResult> SendUpdates(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<SendUpdatesToSalarySubscriptionChatHandler, SendUpdatesToSalarySubscriptionChatCommand, int>(
            new SendUpdatesToSalarySubscriptionChatCommand(id),
            cancellationToken);

        return NoContent();
    }
}