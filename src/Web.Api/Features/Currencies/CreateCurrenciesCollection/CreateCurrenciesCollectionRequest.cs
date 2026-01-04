using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Entities.Salaries;

namespace Web.Api.Features.Currencies.CreateCurrenciesCollection;

public record CreateCurrenciesCollectionRequest
{
    [Required]
    public Dictionary<Currency, double> Currencies { get; init; }

    [Required]
    public DateTime CurrencyDate { get; init; }
}
