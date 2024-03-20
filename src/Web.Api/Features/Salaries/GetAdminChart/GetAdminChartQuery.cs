using MediatR;

namespace TechInterviewer.Features.Salaries.GetAdminChart;

public record GetAdminChartQuery : IRequest<AdminChartResponse>;