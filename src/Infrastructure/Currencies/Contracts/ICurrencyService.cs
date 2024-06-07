using Domain.Entities.Salaries;

namespace Infrastructure.Currencies.Contracts
{
    public interface ICurrencyService
    {
        Task<List<CurrencyContent>> GetCurrenciesAsync(
            List<Currency> currenciesToGet,
            CancellationToken cancellationToken);

        Task<List<CurrencyContent>> GetAllCurrenciesAsync(
            CancellationToken cancellationToken);
    }
}
