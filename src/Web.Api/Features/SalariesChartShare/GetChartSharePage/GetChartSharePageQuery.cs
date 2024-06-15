using MediatR;
using Web.Api.Features.Salaries;

namespace Web.Api.Features.SalariesChartShare.GetChartSharePage;

public record GetChartSharePageQuery
    : SalariesChartQueryParamsBase, IRequest<string>;