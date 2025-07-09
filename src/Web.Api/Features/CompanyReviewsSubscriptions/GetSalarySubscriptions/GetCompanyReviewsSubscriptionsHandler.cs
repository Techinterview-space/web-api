using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviewsSubscriptions.GetSalarySubscriptions;

public class GetCompanyReviewsSubscriptionsHandler
    : IRequestHandler<GetCompanyReviewsSubscriptionsQuery, Pageable<CompanyReviewsSubscriptionDto>>
{
    private readonly DatabaseContext _context;

    public GetCompanyReviewsSubscriptionsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<CompanyReviewsSubscriptionDto>> Handle(
        GetCompanyReviewsSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.CompanyReviewsSubscriptions
            .Include(x => x.TelegramMessages.OrderBy(z => z.CreatedAt))
            .OrderBy(x => x.CreatedAt)
            .Select(CompanyReviewsSubscriptionDto.Transform)
            .AsPaginatedAsync(request, cancellationToken);
    }
}