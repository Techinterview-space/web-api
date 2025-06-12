using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Subscribtions.GetStatDataChangeSubscriptions;

public class GetStatDataChangeSubscriptionsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetStatDataChangeSubscriptionsQuery, Pageable<StatDataChangeSubscriptionDto>>
{
    private readonly DatabaseContext _context;

    public GetStatDataChangeSubscriptionsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<StatDataChangeSubscriptionDto>> Handle(
        GetStatDataChangeSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StatDataChangeSubscriptions
            .Include(x => x.StatDataChangeSubscriptionTgMessages.OrderBy(z => z.CreatedAt))
            .OrderBy(x => x.CreatedAt)
            .Select(StatDataChangeSubscriptionDto.Transform)
            .AsPaginatedAsync(request, cancellationToken);
    }
}