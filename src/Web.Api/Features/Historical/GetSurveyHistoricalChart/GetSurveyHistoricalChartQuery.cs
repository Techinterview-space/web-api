using Web.Api.Features.Historical.GetSalariesHistoricalChart;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record GetSurveyHistoricalChartQuery
    : GetSalariesHistoricalChartQueryParams, MediatR.IRequest<GetSurveyHistoricalChartResponse>;