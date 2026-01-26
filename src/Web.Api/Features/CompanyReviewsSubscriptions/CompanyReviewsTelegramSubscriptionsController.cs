using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.AiServices.Reviews;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.CompanyReviewsSubscriptions.ActivateSubscription;
using Web.Api.Features.CompanyReviewsSubscriptions.CreateSubscription;
using Web.Api.Features.CompanyReviewsSubscriptions.DeactivateSubscription;
using Web.Api.Features.CompanyReviewsSubscriptions.DeleteSubscription;
using Web.Api.Features.CompanyReviewsSubscriptions.EditSubscription;
using Web.Api.Features.CompanyReviewsSubscriptions.GetOpenAiReportAnalysis;
using Web.Api.Features.CompanyReviewsSubscriptions.GetSalaryAiReport;
using Web.Api.Features.CompanyReviewsSubscriptions.GetSalarySubscriptions;
using Web.Api.Features.CompanyReviewsSubscriptions.SendUpdatesToSubscriptionChat;
using Web.Api.Features.SalarySubscribtions.Shared;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.CompanyReviewsSubscriptions;

[ApiController]
[Route("api/company-reviews-telegram-subscriptions")]
[HasAnyRole(Role.Admin)]
public class CompanyReviewsTelegramSubscriptionsController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public CompanyReviewsTelegramSubscriptionsController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("")]
    public async Task<Pageable<CompanyReviewsSubscriptionDto>> Search(
        [FromQuery] GetCompanyReviewsSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetCompanyReviewsSubscriptionsHandler, GetCompanyReviewsSubscriptionsQuery, Pageable<CompanyReviewsSubscriptionDto>>(
            request,
            cancellationToken);
    }

    [HttpPost("")]
    public async Task<CompanyReviewsSubscriptionDto> Create(
        [FromBody] CreateCompanyReviewsSubscriptionBodyRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<CreateCompanyReviewsSubscriptionHandler, CreateCompanyReviewsSubscriptionCommand, CompanyReviewsSubscriptionDto>(
            new CreateCompanyReviewsSubscriptionCommand(
                request),
            cancellationToken);
    }

    [HttpPost("{id:guid}")]
    public async Task<CompanyReviewsSubscriptionDto> Update(
        [FromRoute] Guid id,
        [FromBody] EditSalarySubscriptionBodyRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<EditCompanyReviewsSubscriptionHandler, EditCompanyReviewsSubscriptionCommand, CompanyReviewsSubscriptionDto>(
            new EditCompanyReviewsSubscriptionCommand(
                id,
                request),
            cancellationToken);
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<ActivateCompanyReviewsSubscriptionHandler, ActivateCompanyReviewsSubscriptionCommand, Nothing>(
            new ActivateCompanyReviewsSubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeactivateCompanyReviewsSubscriptionHandler, DeactivateCompanyReviewsSubscriptionCommand, Nothing>(
            new DeactivateCompanyReviewsSubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeleteCompanyReviewsSubscriptionHandler, DeleteCompanyReviewsSubscriptionCommand, Nothing>(
            new DeleteCompanyReviewsSubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/open-ai-analysis")]
    public async Task<IActionResult> GetOpenAiAnalysis(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<GetCompanyReviewsAiReportAnalysisHandler, GetCompanyReviewsAiReportAnalysisQuery, GetCompanyReviewsAiReportAnalysisResponse>(
                new GetCompanyReviewsAiReportAnalysisQuery(id),
                cancellationToken));
    }

    [HttpGet("{id:guid}/open-ai-report")]
    public async Task<IActionResult> GetOpenAiReport(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<GetCompanyReviewsAiReportHandler, GetCompanyReviewsAiReportQuery, CompanyReviewsAiReport>(
                new GetCompanyReviewsAiReportQuery(id),
                cancellationToken));
    }

    [HttpPost("{id:guid}/send-updates")]
    public async Task<IActionResult> SendUpdates(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<SendUpdatesToCompanyReviewSubscriptionChatHandler, SendUpdatesToCompanyReviewsSubscriptionChatCommand, int>(
            new SendUpdatesToCompanyReviewsSubscriptionChatCommand(id),
            cancellationToken);

        return NoContent();
    }
}