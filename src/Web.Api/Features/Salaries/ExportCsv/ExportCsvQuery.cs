using MediatR;

namespace TechInterviewer.Features.Salaries.ExportCsv;

public record ExportCsvQuery : IRequest<SalariesCsvResponse>;