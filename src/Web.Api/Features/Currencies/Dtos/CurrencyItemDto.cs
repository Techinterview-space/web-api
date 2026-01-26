using Domain.Entities.Salaries;

namespace Web.Api.Features.Currencies.Dtos;

public record CurrencyItemDto
{
    public Currency Currency { get; init; }

    public double Value { get; init; }
}
