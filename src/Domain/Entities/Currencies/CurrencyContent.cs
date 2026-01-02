using System;
using System.Globalization;
using System.Xml.Linq;
using Domain.Entities.Salaries;
using Domain.Extensions;

namespace Domain.Entities.Currencies
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
            CurrencyEntity entity)
            : this(
                entity.Value,
                entity.Currency,
                entity.ForDate.Date)
        {
        }

        public CurrencyContent(
            string value,
            string currency,
            string pubDate)
            : this(
                !string.IsNullOrEmpty(value) ?
                    double.Parse(value, CultureInfo.InvariantCulture) :
                    throw new ArgumentException(value, nameof(Value)),
                !string.IsNullOrEmpty(currency) ?
                    currency.ToEnum<Currency>() :
                    throw new ArgumentException(currency, nameof(currency)),
                !string.IsNullOrEmpty(pubDate) ?
                    DateTime.ParseExact(pubDate, "dd.MM.yyyy", CultureInfo.InvariantCulture) :
                    throw new ArgumentException(pubDate, nameof(PubDate)))
        {
        }

        public CurrencyContent(
            double value,
            Currency currency,
            DateTime pubDate)
        {
            Value = value;
            Currency = currency;
            PubDate = pubDate;
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
