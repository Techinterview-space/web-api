using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.SalariesHistoricalDataTemplates.GetTemplates;

public class GetSalariesHistoricalDataRecordTemplatesHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetSalariesHistoricalDataRecordTemplatesQuery, Pageable<SalariesHistoricalDataRecordTemplateDto>>
{
    private readonly DatabaseContext _context;

    public GetSalariesHistoricalDataRecordTemplatesHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<SalariesHistoricalDataRecordTemplateDto>> Handle(
        GetSalariesHistoricalDataRecordTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.SalariesHistoricalDataRecordTemplates
            .OrderByDescending(x => x.CreatedAt)
            .Select(SalariesHistoricalDataRecordTemplateDto.Transform)
            .AsPaginatedAsync(request, cancellationToken);
    }
}