using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Professions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.BackgroundJobs;
using Web.Api.Integrations.OpenAiAnalysisIntegration;

namespace Web.Api.Features.Subscribtions.GetOpenAiReport;

public record GetOpenAiReportHandler : IRequestHandler<GetOpenAiReportQuery, OpenAiBodyReport>
{
    private readonly DatabaseContext _context;
    private readonly IProfessionsCacheService _professionsCacheService;

    public GetOpenAiReportHandler(
        DatabaseContext context,
        IProfessionsCacheService professionsCacheService)
    {
        _context = context;
        _professionsCacheService = professionsCacheService;
    }

    public async Task<OpenAiBodyReport> Handle(
        GetOpenAiReportQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _context
            .StatDataChangeSubscriptions
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

        return new OpenAiBodyReport(data, Currency.KZT);
    }
}