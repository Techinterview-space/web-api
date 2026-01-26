using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.HistoricalRecords;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.SalariesHistoricalDataTemplates.GetTemplates;

namespace Web.Api.Features.SalariesHistoricalDataTemplates.CreateTemplate;

public class CreateSalariesHistoricalDataRecordTemplateHandler
    : IRequestHandler<CreateSalariesHistoricalDataRecordTemplateCommand, SalariesHistoricalDataRecordTemplateDto>
{
    private readonly DatabaseContext _context;

    public CreateSalariesHistoricalDataRecordTemplateHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<SalariesHistoricalDataRecordTemplateDto> Handle(
        CreateSalariesHistoricalDataRecordTemplateCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ProfessionIds == null || request.ProfessionIds.Count == 0)
        {
            throw new BadRequestException("ProfessionIds is required.");
        }

        var professions = await _context.Professions
            .Where(x => request.ProfessionIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (professions.Count != request.ProfessionIds.Count)
        {
            throw new BadRequestException("Some profession IDs are invalid.");
        }

        var newTemplate = _context.Add(
            new SalariesHistoricalDataRecordTemplate(
                request.Name,
                professions));

        await _context.SaveChangesAsync(cancellationToken);
        return new SalariesHistoricalDataRecordTemplateDto(newTemplate.Entity);
    }
}
