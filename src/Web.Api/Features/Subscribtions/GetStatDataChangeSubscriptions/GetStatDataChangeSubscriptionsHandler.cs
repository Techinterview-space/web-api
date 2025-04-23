using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using MediatR;

namespace Web.Api.Features.Subscribtions.GetStatDataChangeSubscriptions;

public class GetStatDataChangeSubscriptionsHandler
    : IRequestHandler<GetStatDataChangeSubscriptionsQuery, Pageable<StatDataChangeSubscriptionDto>>
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
            .OrderBy(x => x.CreatedAt)
            .Select(StatDataChangeSubscriptionDto.Transform)
            .AsPaginatedAsync(request, cancellationToken);
    }
}