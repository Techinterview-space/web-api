using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.AiServices;
using Infrastructure.Services.AiServices.Custom;
using Infrastructure.Services.AiServices.Custom.Models;
using Infrastructure.Services.Html;
using Infrastructure.Services.Professions;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Subscribtions.GetOpenAiReportAnalysis;

public class GetOpenAiReportAnalysisHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetOpenAiReportAnalysisQuery, GetOpenAiReportAnalysisResponse>
{
    private readonly DatabaseContext _context;
    private readonly IProfessionsCacheService _professionsCacheService;
    private readonly ICustomOpenAiService _openApiService;
    private readonly IArtificialIntellectService _aiService;

    public GetOpenAiReportAnalysisHandler(
        DatabaseContext context,
        IProfessionsCacheService professionsCacheService,
        ICustomOpenAiService openApiService,
        IArtificialIntellectService aiService)
    {
        _context = context;
        _professionsCacheService = professionsCacheService;
        _openApiService = openApiService;
        _aiService = aiService;
    }

    public async Task<GetOpenAiReportAnalysisResponse> Handle(
        GetOpenAiReportAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _context
                               .StatDataChangeSubscriptions
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

        var currentTimestamp = Stopwatch.GetTimestamp();
        var analysisResult = await _aiService.AnalyzeSalariesWeeklyUpdateAsync(
            report,
            null,
            cancellationToken);
        var elapsed = Stopwatch.GetElapsedTime(currentTimestamp);

        var rawResponseText = analysisResult.GetResponseTextOrNull();
        var analysis = await _context.SaveAsync(
            new AiAnalysisRecord(
                subscription: subscription,
                aiReportSource: report.ToJson(),
                aiReport: new MarkdownToTelegramHtml(rawResponseText).ToString(),
                processingTimeMs: elapsed.TotalMilliseconds,
                analysisResult.Model),
            cancellationToken);

        return new GetOpenAiReportAnalysisResponse(
            rawResponseText,
            analysis.AiReport,
            report,
            analysisResult.Model);
    }
}