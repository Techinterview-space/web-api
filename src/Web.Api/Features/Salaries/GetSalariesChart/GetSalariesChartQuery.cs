using MediatR;
using TechInterviewer.Features.Salaries.GetSalariesChart.Charts;

namespace TechInterviewer.Features.Salaries.GetSalariesChart;

public record GetSalariesChartQuery
    : SalariesChartQueryParamsBase, IRequest<SalariesChartResponse>;