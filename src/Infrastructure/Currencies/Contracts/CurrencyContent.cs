using System.Globalization;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Domain.Migrations;

namespace Infrastructure.Currencies.Contracts
{
    public record CurrencyContent
    {
        public CurrencyContent(string value, string currency, string pubDate)
        {
            Value = !string.IsNullOrEmpty(value) ?
                        double.Parse(value, CultureInfo.InvariantCulture) :
                        throw new ArgumentException(value, nameof(Value));
            Currency = !string.IsNullOrEmpty(currency) ?
                        EnumHelper.ToEnum<Currency>(currency) :
                        throw new ArgumentException(currency, nameof(currency));
            PubDate = !string.IsNullOrEmpty(pubDate) ?
                        DateTime.ParseExact(pubDate, "dd.MM.yyyy", CultureInfo.InvariantCulture) :
                        throw new ArgumentException(pubDate, nameof(PubDate));
        }

        public double Value { get; set; }
        public Currency Currency { get; set; }
        public DateTime PubDate { get; set; }
    }
}
