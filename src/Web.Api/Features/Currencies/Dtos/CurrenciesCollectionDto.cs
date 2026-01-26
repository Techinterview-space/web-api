using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Currencies;

namespace Web.Api.Features.Currencies.Dtos;

public record CurrenciesCollectionDto
{
    public Guid Id { get; init; }

    public List<CurrencyItemDto> Currencies { get; init; }

    public DateTime CurrencyDate { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public CurrenciesCollectionDto()
    {
    }

    public CurrenciesCollectionDto(CurrenciesCollection entity)
    {
        Id = entity.Id;
        CurrencyDate = entity.CurrencyDate;
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdatedAt;
        Currencies = entity.Currencies
            .Select(x => new CurrencyItemDto
            {
                Currency = x.Key,
                Value = x.Value,
            })
            .ToList();
    }
}
