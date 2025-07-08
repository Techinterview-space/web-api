using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.SalarySubscribtions.GetSalarySubscriptions;

public class GetSalarySubscriptionsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetSalarySubscriptionsQuery, Pageable<SalarySubscriptionDto>>
{
    private readonly DatabaseContext _context;

    public GetSalarySubscriptionsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<SalarySubscriptionDto>> Handle(
        GetSalarySubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StatDataChangeSubscriptions
            .Include(x => x.StatDataChangeSubscriptionTgMessages.OrderBy(z => z.CreatedAt))
            .OrderBy(x => x.CreatedAt)
            .Select(SalarySubscriptionDto.Transform)
            .AsPaginatedAsync(request, cancellationToken);
    }
}