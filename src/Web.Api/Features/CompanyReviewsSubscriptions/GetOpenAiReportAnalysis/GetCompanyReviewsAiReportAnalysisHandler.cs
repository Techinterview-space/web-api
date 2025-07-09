using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.AiServices;
using Infrastructure.Services.AiServices.Reviews;
using Infrastructure.Services.AiServices.Salaries;
using Infrastructure.Services.Html;
using Infrastructure.Services.Professions;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviewsSubscriptions.GetOpenAiReportAnalysis;

public class GetCompanyReviewsAiReportAnalysisHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetCompanyReviewsAiReportAnalysisQuery, GetCompanyReviewsAiReportAnalysisResponse>
{
    private readonly DatabaseContext _context;
    private readonly IArtificialIntellectService _aiService;

    public GetCompanyReviewsAiReportAnalysisHandler(
        DatabaseContext context,
        IArtificialIntellectService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<GetCompanyReviewsAiReportAnalysisResponse> Handle(
        GetCompanyReviewsAiReportAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _context
                               .CompanyReviewsSubscriptions
                               .FirstOrDefaultAsync(
                                   x => x.Id == request.SubscriptionId,
                                   cancellationToken: cancellationToken)
                           ?? throw new NotFoundException($"Subscription {request.SubscriptionId} not found");

        var reviewsForLastWeek = await _context.CompanyReviews
            .Include(x => x.Company)
            .Where(x =>
                x.CreatedAt >= DateTime.UtcNow.AddDays(-7) &&
                x.ApprovedAt != null &&
                x.OutdatedAt == null)
            .Select(CompanyReviewAiReportItem.Transformation)
            .ToListAsync(cancellationToken);

        var report = new CompanyReviewsAiReport(reviewsForLastWeek);

        var currentTimestamp = Stopwatch.GetTimestamp();
        var analysisResult = await _aiService.AnalyzeCompanyReviewsWeeklyUpdateAsync(
            report,
            null,
            cancellationToken);
        var elapsed = Stopwatch.GetElapsedTime(currentTimestamp);

        var rawResponseText = analysisResult.GetResponseTextOrNull();
        var analysis = await _context.SaveAsync(
            new AiAnalysisRecord(
                subscription,
                aiReportSource: report.ToJson(),
                aiReport: new MarkdownToTelegramHtml(rawResponseText).ToString(),
                processingTimeMs: elapsed.TotalMilliseconds,
                analysisResult.Model),
            cancellationToken);

        return new GetCompanyReviewsAiReportAnalysisResponse(
            rawResponseText,
            analysis.AiReport,
            report,
            analysisResult.Model);
    }
}