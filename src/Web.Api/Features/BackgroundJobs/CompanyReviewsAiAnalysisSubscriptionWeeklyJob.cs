using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData;
using Infrastructure.Database;
using Infrastructure.Services.AiServices;
using Infrastructure.Services.AiServices.Reviews;
using Infrastructure.Services.Html;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.BackgroundJobs;

public class CompanyReviewsAiAnalysisSubscriptionWeeklyJob
    : InvocableJobBase<CompanyReviewsAiAnalysisSubscriptionWeeklyJob>
{
    private readonly DatabaseContext _context;
    private readonly IArtificialIntellectService _aiService;

    public CompanyReviewsAiAnalysisSubscriptionWeeklyJob(
        ILogger<CompanyReviewsAiAnalysisSubscriptionWeeklyJob> logger,
        DatabaseContext context,
        IArtificialIntellectService aiService)
        : base(logger)
    {
        _context = context;
        _aiService = aiService;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"job-{Guid.NewGuid()}";
        var subscriptions = await _context.CompanyReviewsSubscriptions
            .Where(x =>
                x.DeletedAt == null &&
                x.UseAiAnalysis)
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
        {
            Logger.LogInformation(
                "No Company Reviews Subscriptions found. Exiting job. CorrelationId: {CorrelationId}",
                correlationId);

            return;
        }

        var results = new List<AiAnalysisRecord>();
        foreach (var subscription in subscriptions)
        {
            var currentTimestamp = Stopwatch.GetTimestamp();

            // TODO mgporbatyuk: implement CompanyReviewsSubscriptionData
            var report = new CompanyReviewsAiReport();
            var analysisResponse = await _aiService.AnalyzeCompanyReviewsWeeklyUpdateAsync(
                report,
                correlationId,
                cancellationToken);

            var elapsed = Stopwatch.GetElapsedTime(currentTimestamp);

            var response = analysisResponse.GetResponseTextOrNull();
            var model = analysisResponse.Model;

            Logger.LogInformation(
                "Company Reviews Subscription {SubscriptionId} analysis completed in {ElapsedMilliseconds} ms. " +
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
                    "Company Reviews Subscription {SubscriptionId} analysis returned empty response. " +
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
            "Company Reviews. Saved {Count} analysis results. CorrelationId: {CorrelationId}",
            results.Count,
            correlationId);
    }
}