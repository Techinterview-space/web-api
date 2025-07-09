using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData;
using Infrastructure.Ai;
using Infrastructure.Database;
using Infrastructure.Services.AiServices;
using Infrastructure.Services.AiServices.Reviews;
using Infrastructure.Services.Html;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web.Api.Services.CompanyReviews;

namespace Web.Api.Features.BackgroundJobs.Companies;

public class CompanyReviewsAiAnalysisSubscriptionWeeklyJob
    : InvocableJobBase<CompanyReviewsAiAnalysisSubscriptionWeeklyJob>
{
    private readonly DatabaseContext _context;
    private readonly IArtificialIntellectService _aiService;

    private readonly CompanyReviewsSubscriptionService _service;

    public CompanyReviewsAiAnalysisSubscriptionWeeklyJob(
        ILogger<CompanyReviewsAiAnalysisSubscriptionWeeklyJob> logger,
        DatabaseContext context,
        IArtificialIntellectService aiService,
        CompanyReviewsSubscriptionService service)
        : base(logger)
    {
        _context = context;
        _aiService = aiService;
        _service = service;
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

        var (analysisResponse, report, elapsedMs) = await GetAnalysisCompanyReviewsWeeklyUpdateOrNullAsync(
            correlationId,
            cancellationToken);

        if (analysisResponse is null)
        {
            return;
        }

        var text = analysisResponse.GetResponseTextOrNull();
        var htmlReport = new MarkdownToTelegramHtml(text).ToString();

        foreach (var subscription in subscriptions)
        {
            var analysis = new AiAnalysisRecord(
                subscription,
                aiReportSource: report.ToJson(),
                aiReport: htmlReport,
                processingTimeMs: elapsedMs,
                analysisResponse.Model);

            _context.Add(analysis);
        }

        await _context.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Company Reviews. Saved {Count} analysis results. CorrelationId: {CorrelationId}",
            subscriptions.Count,
            correlationId);

        await _service.ProcessAllCompanyReviewsSubscriptionsAsync(
            subscriptions
                .Select(x => x.Id)
                .ToList(),
            correlationId,
            cancellationToken);

        Logger.LogInformation(
            "Company Reviews. Job executed. CorrelationId: {CorrelationId}",
            correlationId);
    }

    private async Task<(AiChatResult Result, CompanyReviewsAiReport Report, double ElapsedMs)> GetAnalysisCompanyReviewsWeeklyUpdateOrNullAsync(
        string correlationId,
        CancellationToken cancellationToken)
    {
        var reviewsForLastWeek = await _context.CompanyReviews
            .Include(x => x.Company)
            .Where(x =>
                x.CreatedAt >= DateTime.UtcNow.AddDays(-7) &&
                x.ApprovedAt != null &&
                x.OutdatedAt == null)
            .Select(CompanyReviewAiReportItem.Transformation)
            .ToListAsync(cancellationToken);

        if (reviewsForLastWeek.Count == 0)
        {
            Logger.LogInformation(
                "No Company Reviews for last week found found. Exiting job. CorrelationId: {CorrelationId}",
                correlationId);

            return (null, null, 0);
        }

        var currentTimestamp = Stopwatch.GetTimestamp();

        var report = new CompanyReviewsAiReport(reviewsForLastWeek);
        var analysisResponse = await _aiService.AnalyzeCompanyReviewsWeeklyUpdateAsync(
            report,
            correlationId,
            cancellationToken);

        var elapsed = Stopwatch.GetElapsedTime(currentTimestamp);

        var response = analysisResponse.GetResponseTextOrNull();

        Logger.LogInformation(
            "Company reviews analysis for last week completed in {ElapsedMilliseconds} ms. " +
            "Response length: {ResponseLength}. " +
            "Model: {Model}. " +
            "CorrelationId: {CorrelationId}",
            elapsed.TotalMilliseconds,
            response?.Length,
            analysisResponse.Model,
            correlationId);

        if (response?.Length == 0)
        {
            Logger.LogWarning(
                "Company Reviews analysis returned empty response. " +
                "CorrelationId: {CorrelationId}",
                correlationId);

            return (null, null, 0);
        }

        return (analysisResponse, report, elapsed.TotalMilliseconds);
    }
}