using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;

namespace Domain.Entities.Currencies;

public class CurrenciesCollection : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public Dictionary<Currency, double> Currencies { get; protected set; }

    public DateTime CurrencyDate { get; protected set; }

    public CurrenciesCollection(
        Dictionary<Currency, CurrencyContent> currencies)
    {
        if (currencies.Count == 0)
        {
            throw new InvalidOperationException("Currencies collection must have at least one currency content.");
        }

        Id = Guid.NewGuid();
        CurrencyDate = currencies.First().Value.PubDate;
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;

        Currencies = currencies
            .ToDictionary(
                x => x.Key,
                x => x.Value.Value);
    }

    public List<CurrencyContent> CreateCurrencies()
    {
        var result = Currencies
            .Select(x => new CurrencyContent(
                x.Value,
                x.Key,
                CurrencyDate))
            .ToList();

        if (result.All(x => x.Currency is not Currency.KZT))
        {
            result.Insert(
                0,
                new KztCurrencyContent(CurrencyDate));
        }

        return result;
    }

    protected CurrenciesCollection()
    {
    }
}