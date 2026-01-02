using System;
using Domain.Entities.Salaries;

namespace Domain.Entities.Currencies;

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