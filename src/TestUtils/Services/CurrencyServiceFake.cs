using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Currencies;
using Domain.Entities.Salaries;
using Infrastructure.Currencies.Contracts;

namespace TestUtils.Services;

public class CurrencyServiceFake : ICurrencyService
{
    private readonly List<CurrencyContent> _currencies;

    public CurrencyServiceFake(
        List<CurrencyContent> currencies = null)
    {
        var createdAt = DateTime.UtcNow.AddHours(-1);
        _currencies = currencies ?? new List<CurrencyContent>
        {
            new KztCurrencyContent(createdAt),
            new CurrencyContent(1, Currency.USD, createdAt),
            new CurrencyContent(1, Currency.EUR, createdAt),
            new CurrencyContent(1, Currency.RUB, createdAt),
        };
    }

    public Task<CurrencyContent> GetCurrencyOrNullAsync(Currency currency, CancellationToken cancellationToken)
    {
        var result = _currencies
            .FirstOrDefault(x => x.Currency == currency);

        return Task.FromResult(result);
    }

    public Task<List<CurrencyContent>> GetCurrenciesAsync(List<Currency> currenciesToGet, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            _currencies
                .Where(x => currenciesToGet.Contains(x.Currency))
                .ToList());
    }

    public Task<List<CurrencyContent>> GetAllCurrenciesAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_currencies);
    }

    public Task ResetCacheAsync(CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}