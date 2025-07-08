using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.AiServices;
using Infrastructure.Services.AiServices.Salaries;
using Infrastructure.Services.Html;
using Infrastructure.Services.Professions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.BackgroundJobs;

public class SalariesAiAnalysisSubscriptionWeeklyJob
    : InvocableJobBase<SalariesAiAnalysisSubscriptionWeeklyJob>
{
    private readonly DatabaseContext _context;
    private readonly IProfessionsCacheService _professionsCacheService;
    private readonly IArtificialIntellectService _aiService;

    public SalariesAiAnalysisSubscriptionWeeklyJob(
        ILogger<SalariesAiAnalysisSubscriptionWeeklyJob> logger,
        DatabaseContext context,
        IProfessionsCacheService professionsCacheService,
        IArtificialIntellectService aiService)
        : base(logger)
    {
        _context = context;
        _professionsCacheService = professionsCacheService;
        _aiService = aiService;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"job-{Guid.NewGuid()}";
        var subscriptions = await _context.SalariesSubscriptions
            .Where(x =>
                x.DeletedAt == null &&
                x.UseAiAnalysis)
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
        {
            Logger.LogInformation(
                "No Salary Subscriptions found. Exiting job. CorrelationId: {CorrelationId}",
                correlationId);

            return;
        }

        var allProfessions = await _professionsCacheService.GetProfessionsAsync(cancellationToken);

        var results = new List<AiAnalysisRecord>();
        foreach (var subscription in subscriptions)
        {
            var data = await new SalarySubscriptionData(
                    allProfessions,
                    subscription,
                    _context,
                    DateTimeOffset.UtcNow)
                .InitializeAsync(cancellationToken);

            var report = new SalariesAiBodyReport(data, Currency.KZT);

            var currentTimestamp = Stopwatch.GetTimestamp();

            var analysisResponse = await _aiService.AnalyzeSalariesWeeklyUpdateAsync(
                report,
                correlationId,
                cancellationToken);

            var elapsed = Stopwatch.GetElapsedTime(currentTimestamp);

            var response = analysisResponse.GetResponseTextOrNull();
            var model = analysisResponse.Model;

            Logger.LogInformation(
                "Salary Subscription {SubscriptionId} analysis completed in {ElapsedMilliseconds} ms. " +
                "Response length: {ResponseLength}. " +
                "Model: {Model}. " +
                "CorrelationId: {CorrelationId}",
                subscription.Id,
                elapsed.TotalMilliseconds,
                response.Length,
                model,
                correlationId);

            response = response.Trim('`');
            if (response.Length == 0)
            {
                Logger.LogWarning(
                    "Salary Subscription {SubscriptionId} analysis returned empty response. " +
                    "CorrelationId: {CorrelationId}",
                    subscription.Id,
                    correlationId);

                continue;
            }

            var htmlReport = new MarkdownToTelegramHtml(response).ToString();
            var analysis = new AiAnalysisRecord(
                subscription,
                aiReportSource: report.ToJson(),
                aiReport: htmlReport,
                processingTimeMs: elapsed.TotalMilliseconds,
                model);

            results.Add(analysis);
        }

        _context.AddRange(results);
        await _context.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Salaries. Saved {Count} analysis results. CorrelationId: {CorrelationId}",
            results.Count,
            correlationId);
    }
}