using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.HistoricalRecords;
using Domain.Entities.Salaries;
using Domain.Entities.StatData.Salary;
using Domain.ValueObjects;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.BackgroundJobs.Salaries;

public class SalariesHistoricalDataBackfillJob
    : InvocableJobBase<SalariesHistoricalDataBackfillJob>
{
    private const int MaxDaysPerExecution = 5;
    private readonly DatabaseContext _context;
    private static readonly DateTimeOffset EarliestDate = new (
        new DateTime(2025, 1, 1),
        TimeSpan.Zero);

    public SalariesHistoricalDataBackfillJob(
        ILogger<SalariesHistoricalDataBackfillJob> logger,
        DatabaseContext context)
        : base(logger)
    {
        _context = context;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"backfill-job-{Guid.NewGuid()}";
        Logger.LogInformation(
            "Starting historical data backfill job. CorrelationId: {CorrelationId}",
            correlationId);

        var templatesToBeProcessed = await _context.SalariesHistoricalDataRecordTemplates
            .ToListAsync(cancellationToken);

        if (templatesToBeProcessed.Count == 0)
        {
            Logger.LogInformation(
                "No templates to be processed. CorrelationId: {CorrelationId}",
                correlationId);

            return;
        }

        var now = DateTime.UtcNow;
        var today = new DateTimeOffset(now.Date, TimeSpan.Zero);

        var totalCreatedRecordsCount = 0;

        foreach (var template in templatesToBeProcessed)
        {
            var createdRecordsForDate = 0;

            var earliestRecord = await _context.SalariesHistoricalDataRecords
                .AsNoTracking()
                .Where(x => x.TemplateId == template.Id)
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken);

            var currentDate = earliestRecord != null
                ? earliestRecord.Date.AddDays(-1)
                : today;

            // Go back day by day from today to 2024-01-01, processing up to MaxDaysPerExecution days
            while (currentDate >= EarliestDate && createdRecordsForDate < MaxDaysPerExecution)
            {
                Logger.LogInformation(
                    "Processing date {Date}. CorrelationId: {CorrelationId}",
                    currentDate,
                    correlationId);

                var existingRecord = await _context.SalariesHistoricalDataRecords
                    .AsNoTracking()
                    .Where(x => x.TemplateId == template.Id && x.Date == currentDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingRecord != null)
                {
                    Logger.LogInformation(
                        "Record {RecordId} for template {TemplateId} already exists for date {Date}. CorrelationId: {CorrelationId}",
                        existingRecord.Id,
                        template.Id,
                        currentDate,
                        correlationId);

                    // Move to previous day
                    currentDate = currentDate.AddDays(-1);
                    continue;
                }

                // Use the historical date for the query
                var salariesQuery = new SalariesForChartQuery(
                    _context,
                    new SalariesForChartRequest(template.ProfessionIds ?? new List<long>()),
                    currentDate.DateTime);

                var salaries = await salariesQuery
                    .ToQueryable(CompanyType.Local)
                    .Where(x =>
                        x.Grade.HasValue &&
                        x.SourceType == null)
                    .Select(x => new SalaryBaseData
                    {
                        ProfessionId = x.ProfessionId,
                        Grade = x.Grade.Value,
                        Value = x.Value,
                        CreatedAt = x.CreatedAt,
                    })
                    .ToListAsync(cancellationToken);

                if (salaries.Count == 0)
                {
                    Logger.LogDebug(
                        "No salaries found for template {TemplateId} for date {Date}. CorrelationId: {CorrelationId}",
                        template.Id,
                        currentDate,
                        correlationId);

                    break;
                }

                var dataForRecord = new SalariesStatDataCacheItemSalaryData(
                    salaries,
                    salaries.Count);

                var newRecord = new SalariesHistoricalDataRecord(
                    currentDate,
                    template.Id,
                    dataForRecord);

                _context.Add(newRecord);
                createdRecordsForDate++;

                // Move to previous day
                currentDate = currentDate.AddDays(-1);
            }

            if (createdRecordsForDate > 0)
            {
                var savedCount = await _context.SaveChangesAsync(cancellationToken);
                if (savedCount > 0)
                {
                    totalCreatedRecordsCount += createdRecordsForDate;
                    Logger.LogInformation(
                        "Created {CreatedRecordsCount} new historical data records for date {Date}. CorrelationId: {CorrelationId}",
                        createdRecordsForDate,
                        currentDate,
                        correlationId);
                }
            }
            else
            {
                Logger.LogDebug(
                    "No new records created for date {Date}. CorrelationId: {CorrelationId}",
                    currentDate,
                    correlationId);
            }
        }

        Logger.LogInformation(
            "Historical data backfill job completed, Total records created: {TotalRecordsCreated}. CorrelationId: {CorrelationId}",
            totalCreatedRecordsCount,
            correlationId);
    }

    public record SalariesForChartRequest : SalariesChartQueryParamsBase
    {
        public SalariesForChartRequest(
            List<long> selectedProfessionIds)
        {
            SelectedProfessionIds = selectedProfessionIds;
        }
    }
}