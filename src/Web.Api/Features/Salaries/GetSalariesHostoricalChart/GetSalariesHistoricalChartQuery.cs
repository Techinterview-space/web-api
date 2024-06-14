using MediatR;

namespace TechInterviewer.Features.Salaries.GetSalariesHostoricalChart;

public record GetSalariesHistoricalChartQuery
    : GetSalariesHistoricalChartQueryParams, IRequest<GetSalariesHistoricalChartResponse>;