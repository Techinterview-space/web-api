using MediatR;

namespace Web.Api.Features.Historical.GetSalariesHistoricalChart;

public record GetSalariesHistoricalChartQuery
    : GetSalariesHistoricalChartQueryParams, IRequest<GetSalariesHistoricalChartResponse>;