using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.HistoricalRecords;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.SalariesHistoricalDataTemplates.DeleteAllTemplateRecords;

public class DeleteAllTemplateRecordsHandler
    : IRequestHandler<DeleteAllTemplateRecordsCommand, Nothing>
{
    private readonly DatabaseContext _context;

    public DeleteAllTemplateRecordsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Nothing> Handle(
        DeleteAllTemplateRecordsCommand request,
        CancellationToken cancellationToken)
    {
        var template = await _context.SalariesHistoricalDataRecordTemplates
                          .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                      ?? throw NotFoundException.CreateFromEntity<SalariesHistoricalDataRecordTemplate>(request.Id);

        var records = await _context.SalariesHistoricalDataRecords
            .Where(x => x.TemplateId == template.Id)
            .ToListAsync(cancellationToken);

        if (records.Count > 0)
        {
            _context.SalariesHistoricalDataRecords.RemoveRange(records);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Nothing.Value;
    }
}