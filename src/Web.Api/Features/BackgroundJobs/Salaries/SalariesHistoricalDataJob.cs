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

public class SalariesHistoricalDataJob
    : InvocableJobBase<SalariesHistoricalDataJob>
{
    private readonly DatabaseContext _context;

    public SalariesHistoricalDataJob(
        ILogger<SalariesHistoricalDataJob> logger,
        DatabaseContext context)
        : base(logger)
    {
        _context = context;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"job-{Guid.NewGuid()}";
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

        var createdRecordsCount = 0;
        foreach (var template in templatesToBeProcessed)
        {
            var existingRecordForToday = await _context.SalariesHistoricalDataRecords
                .AsNoTracking()
                .Where(x => x.TemplateId == template.Id && x.Date == today)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingRecordForToday != null)
            {
                Logger.LogInformation(
                    "Record {RecordId} for template {TemplateId} does exist for today {Today}. CorrelationId: {CorrelationId}",
                    existingRecordForToday.Id,
                    template.Id,
                    existingRecordForToday.Date,
                    correlationId);

                continue;
            }

            var salariesQuery = new SalariesForChartQuery(
                    _context,
                    new SalariesForChartRequest(template.ProfessionIds ?? new List<long>()),
                    now);

            var totalSalaryCount = await salariesQuery.CountAsync(cancellationToken);
            var salaries = await salariesQuery
                .ToQueryable(CompanyType.Local)
                .Where(x => x.Grade.HasValue)
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
                Logger.LogInformation(
                    "No salaries found for template {TemplateId} for date {Date}. CorrelationId: {CorrelationId}",
                    template.Id,
                    today,
                    correlationId);

                continue;
            }

            var dataForRecord = new SalariesStatDataCacheItemSalaryData(
                salaries,
                totalSalaryCount);

            var newRecord = new SalariesHistoricalDataRecord(
                today,
                template.Id,
                dataForRecord);

            _context.Add(newRecord);
            createdRecordsCount++;
        }

        if (await _context.SaveChangesAsync(cancellationToken) > 0)
        {
            Logger.LogInformation(
                "Created {CreatedRecordsCount} new historical data records. CorrelationId: {CorrelationId}",
                createdRecordsCount,
                correlationId);
        }
        else
        {
            Logger.LogInformation(
                "No new historical data records were created. CorrelationId: {CorrelationId}",
                correlationId);
        }
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