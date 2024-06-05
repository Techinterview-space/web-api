using System.Xml.Linq;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Infrastructure.Currencies.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Currencies
{
    public class CurrencyService : ICurrencyService
    {
        private const string CacheKey = "CurrencyService_";

        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public CurrencyService(
            IConfiguration configuration,
            IMemoryCache cache)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<List<CurrencyContent>> GetCurrenciesAsync()
        {
            return await _cache.GetOrCreateAsync(
                CacheKey + "_AllCurrencies",
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                    return await GetCurrenciesInternalAsync();
                });
        }

        private async Task<List<CurrencyContent>> GetCurrenciesInternalAsync()
        {
            var url = _configuration["Currencies:Url"];
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException("Currencies url is not set");
            }

            // TODO mgprabtyuk: replace with HttpClientFactory
            using HttpClient client = new HttpClient();
            var xmlContent = await client.GetStringAsync(url);
            var xdoc = XDocument.Parse(xmlContent);

            var items = xdoc.Descendants("item")
                .Select(x => new CurrencyContent(x))
                .Where(x => x.Currency is Currency.USD)
                .ToList();

            return items;
        }
    }
}
