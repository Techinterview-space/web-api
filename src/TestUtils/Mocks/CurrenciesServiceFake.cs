using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Currencies;
using Domain.Entities.Salaries;
using Infrastructure.Currencies.Contracts;

namespace TestUtils.Mocks;

public class CurrenciesServiceFake : ICurrencyService
{
    private readonly List<CurrencyContent> _currencies;

    public CurrenciesServiceFake()
        : this(
            new List<CurrencyContent>
            {
                new CurrencyContent(
                    1,
                    Currency.KZT,
                    DateTime.Now),
                new CurrencyContent(
                    450,
                    Currency.USD,
                    DateTime.Now),
                new CurrencyContent(
                    5.5,
                    Currency.RUB,
                    DateTime.Now),
            })
    {
    }

    public CurrenciesServiceFake(
        List<CurrencyContent> currencies)
    {
        _currencies = currencies;
    }

    public Task<CurrencyContent> GetCurrencyOrNullAsync(
        Currency currency,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_currencies
            .FirstOrDefault(x => x.Currency == currency));
    }

    public Task<List<CurrencyContent>> GetCurrenciesAsync(
        List<Currency> currenciesToGet,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_currencies
            .Where(x => currenciesToGet.Contains(x.Currency))
            .ToList());
    }

    public Task<List<CurrencyContent>> GetAllCurrenciesAsync(
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_currencies);
    }

    public Task RefetchServiceCurrenciesAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}