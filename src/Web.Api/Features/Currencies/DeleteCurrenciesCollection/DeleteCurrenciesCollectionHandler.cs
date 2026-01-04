using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;

namespace Web.Api.Features.Currencies.DeleteCurrenciesCollection;

public class DeleteCurrenciesCollectionHandler
    : IRequestHandler<DeleteCurrenciesCollectionRequest, bool>
{
    private readonly DatabaseContext _context;

    public DeleteCurrenciesCollectionHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        DeleteCurrenciesCollectionRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.CurrencyCollections
            .ByIdOrFailAsync(request.Id, cancellationToken: cancellationToken);

        _context.CurrencyCollections.Remove(entity);
        await _context.TrySaveChangesAsync(cancellationToken);

        return true;
    }
}
