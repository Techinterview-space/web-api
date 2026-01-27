using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.HistoricalRecords;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.SalariesHistoricalDataTemplates.GetTemplates;

namespace Web.Api.Features.SalariesHistoricalDataTemplates.UpdateTemplate;

public class UpdateSalariesHistoricalDataRecordTemplateHandler
    : Infrastructure.Services.Mediator.IRequestHandler<UpdateSalariesHistoricalDataRecordTemplateCommand, SalariesHistoricalDataRecordTemplateDto>
{
    private readonly DatabaseContext _context;

    public UpdateSalariesHistoricalDataRecordTemplateHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<SalariesHistoricalDataRecordTemplateDto> Handle(
        UpdateSalariesHistoricalDataRecordTemplateCommand request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        if (request.ProfessionIds == null || request.ProfessionIds.Count == 0)
        {
            throw new BadRequestException("ProfessionIds is required.");
        }

        var existingTemplate = await _context.SalariesHistoricalDataRecordTemplates
            .FirstOrDefaultAsync(
                x => x.Id == request.TemplateId,
                cancellationToken);

        if (existingTemplate == null)
        {
            throw NotFoundException.CreateFromEntity<SalariesHistoricalDataRecordTemplate>(request.TemplateId);
        }

        var professions = await _context.Professions
            .Where(x => request.ProfessionIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (professions.Count != request.ProfessionIds.Count)
        {
            throw new BadRequestException("Some profession IDs are invalid.");
        }

        existingTemplate.Update(
            request.Name,
            professions);

        await _context.SaveChangesAsync(cancellationToken);
        return new SalariesHistoricalDataRecordTemplateDto(existingTemplate);
    }
}