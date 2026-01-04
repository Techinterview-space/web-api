using System;

namespace Web.Api.Features.Currencies.DeleteCurrenciesCollection;

public record DeleteCurrenciesCollectionRequest
{
    public Guid Id { get; init; }
}
