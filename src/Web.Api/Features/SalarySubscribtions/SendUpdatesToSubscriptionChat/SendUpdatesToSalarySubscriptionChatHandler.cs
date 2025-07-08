using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Correlation;
using Microsoft.EntityFrameworkCore;
using Web.Api.Services.Salaries;

namespace Web.Api.Features.SalarySubscribtions.SendUpdatesToSubscriptionChat;

public class SendUpdatesToSalarySubscriptionChatHandler
    : Infrastructure.Services.Mediator.IRequestHandler<SendUpdatesToSalarySubscriptionChatCommand, int>
{
    private readonly StatDataChangeSubscriptionService _service;
    private readonly DatabaseContext _context;
    private readonly ICorrelationIdAccessor _correlation;

    public SendUpdatesToSalarySubscriptionChatHandler(
        StatDataChangeSubscriptionService service,
        DatabaseContext context,
        ICorrelationIdAccessor correlation)
    {
        _service = service;
        _context = context;
        _correlation = correlation;
    }

    public async Task<int> Handle(
        SendUpdatesToSalarySubscriptionChatCommand request,
        CancellationToken cancellationToken)
    {
        var hasSubscription = await _context.StatDataChangeSubscriptions
            .AnyAsync(x => x.Id == request.SubscriptionId, cancellationToken);

        if (!hasSubscription)
        {
            throw new NotFoundException($"Subscription {request.SubscriptionId} not found.");
        }

        var result = await _service.ProcessSalarySubscriptionAsync(
            request.SubscriptionId,
            _correlation.GetValue(),
            cancellationToken);

        return result;
    }
}