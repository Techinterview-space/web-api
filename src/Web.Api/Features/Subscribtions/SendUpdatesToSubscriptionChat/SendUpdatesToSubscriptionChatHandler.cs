using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Correlation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Services.Salaries;

namespace Web.Api.Features.Subscribtions.SendUpdatesToSubscriptionChat;

public class SendUpdatesToSubscriptionChatHandler
    : IRequestHandler<SendUpdatesToSubscriptionChatCommand, int>
{
    private readonly StatDataChangeSubscriptionService _service;
    private readonly DatabaseContext _context;
    private readonly ICorrelationIdAccessor _correlation;

    public SendUpdatesToSubscriptionChatHandler(
        StatDataChangeSubscriptionService service,
        DatabaseContext context,
        ICorrelationIdAccessor correlation)
    {
        _service = service;
        _context = context;
        _correlation = correlation;
    }

    public async Task<int> Handle(
        SendUpdatesToSubscriptionChatCommand request,
        CancellationToken cancellationToken)
    {
        var hasSubscription = await _context.StatDataChangeSubscriptions
            .AnyAsync(x => x.Id == request.SubscriptionId, cancellationToken);

        if (!hasSubscription)
        {
            throw new NotFoundException($"Subscription {request.SubscriptionId} not found.");
        }

        var result = await _service.ProcessSubscriptionAsync(
            request.SubscriptionId,
            _correlation.GetValue(),
            cancellationToken);

        return result;
    }
}