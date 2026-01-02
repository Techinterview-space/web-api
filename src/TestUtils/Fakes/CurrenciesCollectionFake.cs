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
        List<CurrencyContent> currencies = null)
        : base(
            currencies ?? CreateDefaultCurrencies(currencyDate))
    {
    }

    public static List<CurrencyContent> CreateDefaultCurrencies(
        DateTime currencyDate)
    {
        return new List<CurrencyContent>()
        {
            new CurrencyContent(
                400,
                Currency.USD,
                currencyDate),
            new CurrencyContent(
                500,
                Currency.EUR,
                currencyDate),
            new KztCurrencyContent(currencyDate),
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