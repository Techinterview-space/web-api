﻿using Domain.Entities.Salaries;

namespace Infrastructure.Currencies.Contracts
{
    public interface ICurrencyService
    {
        Task<CurrencyContent> GetCurrencyAsync(
            Currency currency,
            CancellationToken cancellationToken);

        Task<List<CurrencyContent>> GetCurrenciesAsync(
            List<Currency> currenciesToGet,
            CancellationToken cancellationToken);

        Task<List<CurrencyContent>> GetAllCurrenciesAsync(
            CancellationToken cancellationToken);

        Task ResetCacheAsync(
            CancellationToken cancellationToken);
    }
}
