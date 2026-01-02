using Domain.Entities.Currencies;

namespace Infrastructure.Currencies;

public interface ICurrenciesHttpService
{
    Task<List<CurrencyContent>> GetCurrenciesAsync(
        CancellationToken cancellationToken);
}