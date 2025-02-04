using Domain.ValueObjects;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record PeopleAgeValuesSplitter : ValuesByRangesSplitter
{
    public PeopleAgeValuesSplitter()
        : base(15, 65, 5)
    {
    }
}