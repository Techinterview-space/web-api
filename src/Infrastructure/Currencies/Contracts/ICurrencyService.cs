namespace Infrastructure.Currencies.Contracts
{
    public interface ICurrencyService
    {
        Task<List<CurrencyContent>> GetCurrenciesAsync(
            CancellationToken cancellationToken);
    }
}
