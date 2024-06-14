using MediatR;

namespace TechInterviewer.Features.Salaries.GetSalariesHistoricalChart;

public record GetSalariesHistoricalChartQuery
    : GetSalariesHistoricalChartQueryParams, IRequest<GetSalariesHistoricalChartResponse>;