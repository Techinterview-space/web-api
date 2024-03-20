using MediatR;

namespace TechInterviewer.Features.Salaries.GetChartSharePage;

public record GetChartSharePageQuery
    : SalariesChartQueryParamsBase, IRequest<string>;