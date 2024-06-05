using System.Globalization;
using System.Xml.Linq;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Domain.Migrations;

namespace Infrastructure.Currencies.Contracts
{
    public record CurrencyContent
    {
        public CurrencyContent(
            XElement item)
            : this(
                item.Element("description")?.Value,
                item.Element("title")?.Value,
                item.Element("pubDate")?.Value)
        {
        }

        public CurrencyContent(
            string value,
            string currency,
            string pubDate)
        {
            Value = !string.IsNullOrEmpty(value) ?
                        double.Parse(value, CultureInfo.InvariantCulture) :
                        throw new ArgumentException(value, nameof(Value));

            Currency = !string.IsNullOrEmpty(currency) ?
                        currency.ToEnum<Currency>() :
                        throw new ArgumentException(currency, nameof(currency));

            PubDate = !string.IsNullOrEmpty(pubDate) ?
                        DateTime.ParseExact(pubDate, "dd.MM.yyyy", CultureInfo.InvariantCulture) :
                        throw new ArgumentException(pubDate, nameof(PubDate));
        }

        public double Value { get; }

        public Currency Currency { get; }

        public string CurrencyString
        {
            get
            {
                return Currency switch
                {
                    Currency.USD => "$",
                    _ => Currency.ToString().ToLowerInvariant()
                };
            }
        }

        public DateTime PubDate { get; }
    }
}
