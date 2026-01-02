using System;
using System.Collections.Generic;
using Domain.Entities.Currencies;
using Domain.Entities.Salaries;
using TestUtils.Db;

namespace TestUtils.Fakes;

public class CurrenciesCollectionFake : CurrenciesCollection
{
    public CurrenciesCollectionFake(
        DateTime currencyDate,
        Dictionary<Currency, CurrencyContent> currencies = null)
        : base(
            currencies ?? CreateDefaultCurrencies(currencyDate))
    {
    }

    public static Dictionary<Currency, CurrencyContent> CreateDefaultCurrencies(
        DateTime currencyDate)
    {
        return new Dictionary<Currency, CurrencyContent>
        {
            {
                Currency.USD,
                new CurrencyContent(
                    400,
                    Currency.USD,
                    currencyDate)
            },
            {
                Currency.EUR,
                new CurrencyContent(
                    500,
                    Currency.EUR,
                    currencyDate)
            },
            {
                Currency.KZT,
                new KztCurrencyContent(currencyDate)
            }
        };
    }

    public CurrenciesCollection Please(
        InMemoryDatabaseContext context)
    {
        var entry = context.Add((CurrenciesCollection)this);
        context.SaveChanges();
        return entry.Entity;
    }
}