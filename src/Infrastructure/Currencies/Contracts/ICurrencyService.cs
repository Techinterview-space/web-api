using Domain.Entities.Currencies;
using Domain.Entities.Salaries;

namespace Infrastructure.Currencies.Contracts
{
    public interface ICurrencyService
    {
        Task<CurrencyContent> GetCurrencyOrNullAsync(
            Currency currency,
            CancellationToken cancellationToken);

        Task<List<CurrencyContent>> GetAllCurrenciesAsync(
            CancellationToken cancellationToken);

        Task RefetchServiceCurrenciesAsync(
            CancellationToken cancellationToken);
    }
}
