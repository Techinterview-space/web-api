using MediatR;
using TechInterviewer.Features.Salaries;

namespace TechInterviewer.Features.SalariesChartShare.GetChartSharePage;

public record GetChartSharePageQuery
    : SalariesChartQueryParamsBase, IRequest<string>;