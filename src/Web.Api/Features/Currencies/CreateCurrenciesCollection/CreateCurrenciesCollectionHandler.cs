using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Currencies;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Currencies.Dtos;

namespace Web.Api.Features.Currencies.CreateCurrenciesCollection;

public class CreateCurrenciesCollectionHandler(
    DatabaseContext context)
    : IRequestHandler<CreateCurrenciesCollectionRequest, CurrenciesCollectionDto>
{
    public async Task<CurrenciesCollectionDto> Handle(
        CreateCurrenciesCollectionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Currencies == null || request.Currencies.Count == 0)
        {
            throw new BadRequestException("Currencies collection must have at least one currency.");
        }

        if (request.CurrencyDate > System.DateTime.UtcNow.Date)
        {
            throw new BadRequestException("Currency date cannot be in the future.");
        }

        var existingRecord = await context.CurrencyCollections
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CurrencyDate == request.CurrencyDate, cancellationToken);

        if (existingRecord != null)
        {
            throw new BadRequestException($"A currency collection for date {request.CurrencyDate:yyyy-MM-dd} already exists.");
        }

        var currencyContents = request.Currencies
            .Select(x => new CurrencyContent(x.Value, x.Key, request.CurrencyDate))
            .ToDictionary(x => x.Currency, x => x);

        var entity = new CurrenciesCollection(currencyContents);

        context.CurrencyCollections.Add(entity);
        await context.TrySaveChangesAsync(cancellationToken);

        return new CurrenciesCollectionDto(entity);
    }
}
