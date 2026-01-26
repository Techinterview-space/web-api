using Domain.Entities.Currencies;
using Domain.Entities.Salaries;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Currencies
{
    public class CurrencyService : ICurrencyService
    {
        private const string CacheKey = "CurrencyService__AllCurrencies";

        private readonly ICurrenciesHttpService _currenciesHttpService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyService> _logger;
        private readonly DatabaseContext _context;

        public CurrencyService(
            ICurrenciesHttpService currenciesHttpService,
            IMemoryCache cache,
            ILogger<CurrencyService> logger,
            DatabaseContext context)
        {
            _currenciesHttpService = currenciesHttpService;
            _cache = cache;
            _logger = logger;
            _context = context;
        }

        public async Task<CurrencyContent> GetCurrencyOrNullAsync(
            Currency currency,
            CancellationToken cancellationToken)
        {
            var allCurrencies = await GetAllCurrenciesAsync(cancellationToken);
            return allCurrencies
                .FirstOrDefault(x => x.Currency == currency);
        }

        public async Task<List<CurrencyContent>> GetAllCurrenciesAsync(
            CancellationToken cancellationToken)
        {
            return await _cache.GetOrCreateAsync(
                CacheKey,
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);

                    var latestCurrenciesCollection = await _context.CurrencyCollections
                        .OrderByDescending(x => x.CurrencyDate)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (latestCurrenciesCollection is not null)
                    {
                        return latestCurrenciesCollection.CreateCurrencies();
                    }

                    var recreatedCurrenciesCollection = await RecreateCurrenciesCollectionOrNullAsync(
                        DateTime.UtcNow,
                        cancellationToken);

                    if (recreatedCurrenciesCollection is not null)
                    {
                        return recreatedCurrenciesCollection.CreateCurrencies();
                    }

                    return CreateDefaultCollection();
                });
        }

        public async Task RefetchServiceCurrenciesAsync(
            CancellationToken cancellationToken)
        {
            await RecreateCurrenciesCollectionOrNullAsync(
                DateTime.UtcNow,
                cancellationToken);

            if (_cache.TryGetValue(CacheKey, out _))
            {
                _cache.Remove(CacheKey);
            }
        }

        private async Task<CurrenciesCollection> RecreateCurrenciesCollectionOrNullAsync(
            DateTime forDate,
            CancellationToken cancellationToken)
        {
            var dateForQuery = forDate.Date;
            var existingEntity = await _context.CurrencyCollections
                .FirstOrDefaultAsync(
                    x => x.CurrencyDate == dateForQuery,
                    cancellationToken);

            if (existingEntity is null)
            {
                var currencies = await _currenciesHttpService.GetCurrenciesAsync(cancellationToken);
                if (currencies.Count == 0)
                {
                    return null;
                }

                var newEntity = new CurrenciesCollection(currencies);

                _context.CurrencyCollections.Add(newEntity);
                await _context.SaveChangesAsync(cancellationToken);

                return newEntity;
            }

            _logger.LogInformation(
                "There is an existing record for date {Date} ({Currencies}) [ID:{RecordId}]",
                existingEntity.CurrencyDate.ToString("O"),
                string.Join(", ", existingEntity.Currencies.Select(x => x.Key)),
                existingEntity.Id);

            return existingEntity;
        }

        private List<CurrencyContent> CreateDefaultCollection()
        {
            return new List<CurrencyContent>
            {
                new KztCurrencyContent(DateTime.UtcNow),
            };
        }
    }
}
