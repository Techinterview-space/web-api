using System;
using Domain.Entities.Salaries;

namespace Domain.Entities.Currencies;

public class CurrencyEntity : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public double Value { get; protected set; }

    public Currency Currency { get; protected set; }

    public DateTimeOffset ForDate { get; protected set; }

    public CurrencyEntity(
        double value,
        Currency currency,
        DateTimeOffset forDate)
    {
        Id = Guid.NewGuid();
        Value = value;
        Currency = currency;
        ForDate = new DateTimeOffset(
            forDate.Year,
            forDate.Month,
            forDate.Day,
            12,
            0,
            0,
            TimeSpan.Zero);

        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    public CurrencyContent CreateCurrencyContent()
    {
        return new CurrencyContent(this);
    }

    protected CurrencyEntity()
    {
    }
}