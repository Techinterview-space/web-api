using Domain.Entities.Currencies;
using Domain.Entities.Salaries;

namespace Infrastructure.Currencies;

public interface ICurrenciesHttpService
{
    Task<Dictionary<Currency, CurrencyContent>> GetCurrenciesAsync(
        CancellationToken cancellationToken);
}