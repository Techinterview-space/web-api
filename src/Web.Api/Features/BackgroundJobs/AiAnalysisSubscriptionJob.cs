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
using Infrastructure.Services.OpenAi;
using Infrastructure.Services.OpenAi.Models;
using Infrastructure.Services.Professions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.BackgroundJobs;

public class AiAnalysisSubscriptionJob
    : InvocableJobBase<AiAnalysisSubscriptionJob>
{
    private readonly DatabaseContext _context;
    private readonly IProfessionsCacheService _professionsCacheService;
    private readonly IOpenAiService _openAiService;

    public AiAnalysisSubscriptionJob(
        ILogger<AiAnalysisSubscriptionJob> logger,
        DatabaseContext context,
        IProfessionsCacheService professionsCacheService,
        IOpenAiService openAiService)
        : base(logger)
    {
        _context = context;
        _professionsCacheService = professionsCacheService;
        _openAiService = openAiService;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _context.StatDataChangeSubscriptions
            .Where(x =>
                x.DeletedAt == null &&
                x.RequestAiAnalysis)
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
        {
            Logger.LogInformation(
                "No Subscriptions found. Exiting job.");
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

            var report = new OpenAiBodyReport(data, Currency.KZT);

            var currentTimestamp = Stopwatch.GetTimestamp();
            var response = await _openAiService.GetAnalysisAsync(
                report,
                cancellationToken);

            var elapsed = Stopwatch.GetElapsedTime(currentTimestamp);

            Logger.LogInformation(
                "Subscription {SubscriptionId} analysis completed in {ElapsedMilliseconds} ms. Response length: {ResponseLength}",
                subscription.Id,
                elapsed.TotalMilliseconds,
                response.Length);

            response = response.Trim('`');
            if (response.Length == 0)
            {
                Logger.LogWarning(
                    "Subscription {SubscriptionId} analysis returned empty response.",
                    subscription.Id);
                continue;
            }

            var analysis = new AiAnalysisRecord(
                subscription,
                response,
                report.ToJson(),
                elapsed.TotalMilliseconds);

            results.Add(analysis);
        }

        _context.AddRange(results);
        await _context.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Saved {Count} analysis results.",
            results.Count);
    }
}