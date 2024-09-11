using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using MediatR;

namespace Web.Api.Features.Telegram.GetStatDataCacheRecords;

public class GetGetStatDataCacheHandler
    : IRequestHandler<GetStatDataCacheQuery, Pageable<StatDataCacheDto>>
{
    private readonly DatabaseContext _context;

    public GetGetStatDataCacheHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<StatDataCacheDto>> Handle(
        GetStatDataCacheQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.StatDataCacheRecords
            .OrderBy(x => x.CreatedAt)
            .Select(StatDataCacheDto.Transform)
            .AsPaginatedAsync(request, cancellationToken);
    }
}