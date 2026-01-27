using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Correlation;
using Microsoft.EntityFrameworkCore;
using Web.Api.Services.CompanyReviews;

namespace Web.Api.Features.CompanyReviewsSubscriptions.SendUpdatesToSubscriptionChat;

public class SendUpdatesToCompanyReviewSubscriptionChatHandler
    : Infrastructure.Services.Mediator.IRequestHandler<SendUpdatesToCompanyReviewsSubscriptionChatCommand, int>
{
    private readonly CompanyReviewsSubscriptionService _service;
    private readonly DatabaseContext _context;
    private readonly ICorrelationIdAccessor _correlation;

    public SendUpdatesToCompanyReviewSubscriptionChatHandler(
        CompanyReviewsSubscriptionService service,
        DatabaseContext context,
        ICorrelationIdAccessor correlation)
    {
        _service = service;
        _context = context;
        _correlation = correlation;
    }

    public async Task<int> Handle(
        SendUpdatesToCompanyReviewsSubscriptionChatCommand request,
        CancellationToken cancellationToken)
    {
        var hasSubscription = await _context.CompanyReviewsSubscriptions
            .AnyAsync(x => x.Id == request.SubscriptionId, cancellationToken);

        if (!hasSubscription)
        {
            throw new NotFoundException($"Subscription {request.SubscriptionId} not found.");
        }

        var result = await _service.ProcessCompanyReviewsSubscriptionAsync(
            request.SubscriptionId,
            _correlation.GetValue(),
            cancellationToken);

        return result;
    }
}