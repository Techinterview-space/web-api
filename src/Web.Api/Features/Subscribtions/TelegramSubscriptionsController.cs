using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Subscribtions.ActivateSubscription;
using Web.Api.Features.Subscribtions.CreateSubscription;
using Web.Api.Features.Subscribtions.DeactivateSubscription;
using Web.Api.Features.Subscribtions.DeleteSubscription;
using Web.Api.Features.Subscribtions.GetStatDataChangeSubscriptions;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Subscribtions;

[ApiController]
[Route("api/telegram-subscriptions")]
[HasAnyRole(Role.Admin)]
public class TelegramSubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TelegramSubscriptionsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<Pageable<StatDataChangeSubscriptionDto>> Search(
        [FromQuery] GetStatDataChangeSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            request,
            cancellationToken);
    }

    [HttpPost("")]
    public async Task<StatDataChangeSubscriptionDto> Create(
        [FromBody] CreateSubscriptionBodyRequest request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new CreateSubscriptionCommand(
                request),
            cancellationToken);
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ActivateStatDataChangeSubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeactivateStatDataChangeSubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteSubscriptionCommand(id),
            cancellationToken);

        return NoContent();
    }
}