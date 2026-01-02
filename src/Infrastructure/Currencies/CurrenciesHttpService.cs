using System.Xml.Linq;
using Domain.Entities.Currencies;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Currencies;

public class CurrenciesHttpService : ICurrenciesHttpService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CurrenciesHttpService> _logger;

    public CurrenciesHttpService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<CurrenciesHttpService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Dictionary<Currency, CurrencyContent>> GetCurrenciesAsync(
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
            return xdoc.Descendants("item")
                .Select(x => new CurrencyContent(x))
                .Where(x => currenciesToSave.Contains(x.Currency))
                .GroupBy(x => x.Currency)
                .ToDictionary(x => x.Key, x => x.First());
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error while getting currencies from {Url}. Message {Message}",
                currenciesUrl,
                e.Message);

            return new Dictionary<Currency, CurrencyContent>(0);
        }
    }
}