using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.OpenAi;
using Infrastructure.Services.OpenAi.Models;
using Infrastructure.Services.Professions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Subscribtions.GetOpenAiReportAnalysis;

public class GetOpenAiReportAnalysisHandler : IRequestHandler<GetOpenAiReportAnalysisQuery, GetOpenAiReportAnalysisResponse>
{
    private readonly DatabaseContext _context;
    private readonly IProfessionsCacheService _professionsCacheService;
    private readonly IOpenAiService _openApiService;

    public GetOpenAiReportAnalysisHandler(
        DatabaseContext context,
        IProfessionsCacheService professionsCacheService,
        IOpenAiService openApiService)
    {
        _context = context;
        _professionsCacheService = professionsCacheService;
        _openApiService = openApiService;
    }

    public async Task<GetOpenAiReportAnalysisResponse> Handle(
        GetOpenAiReportAnalysisQuery request,
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

        var report = new OpenAiBodyReport(data, Currency.KZT);
        return new GetOpenAiReportAnalysisResponse(
            await _openApiService.GetAnalysisAsync(report, cancellationToken),
            report,
            _openApiService.GetBearer());
    }
}