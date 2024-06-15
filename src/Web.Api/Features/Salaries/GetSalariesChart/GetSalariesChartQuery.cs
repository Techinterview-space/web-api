using MediatR;
using Web.Api.Features.Salaries.GetSalariesChart.Charts;

namespace Web.Api.Features.Salaries.GetSalariesChart;

public record GetSalariesChartQuery
    : SalariesChartQueryParamsBase, IRequest<SalariesChartResponse>;