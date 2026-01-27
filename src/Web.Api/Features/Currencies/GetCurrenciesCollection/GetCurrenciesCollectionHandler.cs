using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Currencies.Dtos;

namespace Web.Api.Features.Currencies.GetCurrenciesCollection;

public class GetCurrenciesCollectionHandler
    : IRequestHandler<GetCurrenciesCollectionQueryParams, Pageable<CurrenciesCollectionDto>>
{
    private readonly DatabaseContext _context;

    public GetCurrenciesCollectionHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<CurrenciesCollectionDto>> Handle(
        GetCurrenciesCollectionQueryParams request,
        CancellationToken cancellationToken)
    {
        return await _context.CurrencyCollections
            .AsNoTracking()
            .OrderByDescending(x => x.CurrencyDate)
            .AsPaginatedAsync(
                x => new CurrenciesCollectionDto(x),
                request,
                cancellationToken);
    }
}
