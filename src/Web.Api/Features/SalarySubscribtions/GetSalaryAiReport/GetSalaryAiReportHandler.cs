using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.AiServices.Salaries;
using Infrastructure.Services.Professions;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.SalarySubscribtions.GetSalaryAiReport;

public record GetSalaryAiReportHandler : Infrastructure.Services.Mediator.IRequestHandler<GetSalaryAiReportQuery, SalariesAiBodyReport>
{
    private readonly DatabaseContext _context;
    private readonly IProfessionsCacheService _professionsCacheService;

    public GetSalaryAiReportHandler(
        DatabaseContext context,
        IProfessionsCacheService professionsCacheService)
    {
        _context = context;
        _professionsCacheService = professionsCacheService;
    }

    public async Task<SalariesAiBodyReport> Handle(
        GetSalaryAiReportQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _context
            .SalariesSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.SubscriptionId,
                cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Subscription {request.SubscriptionId} not found");

        var allProfessions = await _professionsCacheService.GetProfessionsAsync(cancellationToken);
        var data = await new SalarySubscriptionData(
            allProfessions,
            subscription,
            _context,
            DateTimeOffset.UtcNow)
            .InitializeAsync(cancellationToken);

        return new SalariesAiBodyReport(data, Currency.KZT);
    }
}