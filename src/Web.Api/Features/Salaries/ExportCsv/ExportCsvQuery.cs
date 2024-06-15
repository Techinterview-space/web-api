using MediatR;

namespace Web.Api.Features.Salaries.ExportCsv;

public record ExportCsvQuery : IRequest<SalariesCsvResponse>;