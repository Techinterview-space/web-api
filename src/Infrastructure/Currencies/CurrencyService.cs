﻿using System.Xml.Linq;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Infrastructure.Currencies.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Currencies
{
    public class CurrencyService : ICurrencyService
    {
        private const string CacheKey = "CurrencyService__AllCurrencies";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<CurrencyService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CurrencyContent> GetCurrencyOrNullAsync(
            Currency currency,
            CancellationToken cancellationToken)
        {
            return (await GetCurrenciesAsync(
                    [currency],
                    cancellationToken))
                .FirstOrDefault();
        }

        public async Task<List<CurrencyContent>> GetCurrenciesAsync(
            List<Currency> currenciesToGet,
            CancellationToken cancellationToken)
        {
            if (currenciesToGet == null || !currenciesToGet.Any())
            {
                return new List<CurrencyContent>();
            }

            var currencies = await _cache.GetOrCreateAsync(
                CacheKey,
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                    return await GetCurrenciesInternalAsync(cancellationToken);
                });

            return currencies
                .Where(x => currenciesToGet.Contains(x.Currency))
                .ToList();
        }

        public async Task<List<CurrencyContent>> GetAllCurrenciesAsync(
            CancellationToken cancellationToken)
        {
            return await _cache.GetOrCreateAsync(
                CacheKey,
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                    return await GetCurrenciesInternalAsync(cancellationToken);
                });
        }

        public async Task ResetCacheAsync(
            CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(CacheKey, out _))
            {
                _cache.Remove(CacheKey);
            }

            await GetAllCurrenciesAsync(cancellationToken);
        }

        private async Task<List<CurrencyContent>> GetCurrenciesInternalAsync(
            CancellationToken cancellationToken)
        {
            var currenciesUrl = _configuration["Currencies:Url"];
            if (string.IsNullOrEmpty(currenciesUrl))
            {
                throw new InvalidOperationException("Currencies url is not set");
            }

            try
            {
                using var client = _httpClientFactory.CreateClient();
                var xmlContent = await client.GetStringAsync(currenciesUrl, cancellationToken);
                var xdoc = XDocument.Parse(xmlContent);

                var currenciesToSave = EnumHelper.Values<Currency>(true);
                var items = xdoc.Descendants("item")
                    .Select(x => new CurrencyContent(x))
                    .Where(x => currenciesToSave.Contains(x.Currency))
                    .ToList();

                if (items.All(x => x.Currency is not Currency.KZT))
                {
                    var pubDate = items.FirstOrDefault()?.PubDate ?? DateTime.UtcNow;
                    items.Insert(
                        0,
                        new KztCurrencyContent(pubDate));
                }

                return items;
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "Error while getting currencies from {Url}. Message {Message}",
                    currenciesUrl,
                    e.Message);

                return new List<CurrencyContent>
                {
                    new KztCurrencyContent(DateTime.UtcNow),
                };
            }
        }
    }
}
