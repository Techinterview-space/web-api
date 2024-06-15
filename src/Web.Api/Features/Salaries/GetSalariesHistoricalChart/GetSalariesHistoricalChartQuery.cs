using MediatR;

namespace Web.Api.Features.Salaries.GetSalariesHistoricalChart;

public record GetSalariesHistoricalChartQuery
    : GetSalariesHistoricalChartQueryParams, IRequest<GetSalariesHistoricalChartResponse>;