using Domain.Entities.Salaries;

namespace Infrastructure.Currencies.Contracts;

public record KztCurrencyContent : CurrencyContent
{
    public KztCurrencyContent(
        DateTime createdAt)
        : base(
            1,
            Currency.KZT,
            createdAt)
    {
    }
}