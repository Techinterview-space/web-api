using Domain.ValueObjects;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record UserExperienceValuesSplitter : ValuesByRangesSplitter
{
    public UserExperienceValuesSplitter()
        : base(0, 20, 1)
    {
    }
}