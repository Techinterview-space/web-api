using System.Xml.Linq;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Infrastructure.Currencies.Contracts;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Currencies
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IConfiguration _configuration;

        public CurrencyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<CurrencyContent>> GetCurrencies()
        {
            var url = _configuration["Currencies:Url"];

            using HttpClient client = new HttpClient();
            string xmlContent = await client.GetStringAsync(url);
            XDocument xdoc = XDocument.Parse(xmlContent);

            var items = xdoc.Descendants("item")
                .Select(ParseItem)
                .Where(IsIncludedCurrencyItem)
                .ToList();

            return items;
        }

        private CurrencyContent ParseItem(XElement item)
        {
            return new CurrencyContent(
                item.Element("description")?.Value,
                item.Element("title")?.Value,
                item.Element("pubDate")?.Value);
        }

        private bool IsIncludedCurrencyItem(CurrencyContent item)
        {
            return item.Currency == Currency.RUB ||
                   item.Currency == Currency.USD;
        }
    }
}
