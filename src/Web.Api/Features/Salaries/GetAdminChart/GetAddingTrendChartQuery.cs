using MediatR;

namespace TechInterviewer.Features.Salaries.GetAdminChart;

public record GetAddingTrendChartQuery : IRequest<GetAddingTrendChartResponse>;