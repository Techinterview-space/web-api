using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Historical.GetSalariesHistoricalChart;

public class GetSalariesHistoricalChartHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetSalariesHistoricalChartQueryParams, GetSalariesHistoricalChartResponse>
{
    private readonly DatabaseContext _context;

    public GetSalariesHistoricalChartHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<GetSalariesHistoricalChartResponse> Handle(
        GetSalariesHistoricalChartQueryParams request,
        CancellationToken cancellationToken)
    {
        var to = request.To ?? DateTimeOffset.UtcNow;
        var from = request.From ?? to.AddMonths(-12);

        // Query historical data records within the date range
        var historicalRecords = await _context.SalariesHistoricalDataRecords
            .AsNoTracking()
            .Include(x => x.SalariesHistoricalDataRecordTemplate)
            .Where(x => x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        // Group by template
        var groupedByTemplate = historicalRecords
            .GroupBy(x => x.TemplateId)
            .Select(group =>
            {
                var template = group.First().SalariesHistoricalDataRecordTemplate;

                // Group by week and aggregate data
                var dataPoints = group
                    .GroupBy(record => GetStartOfWeek(record.Date))
                    .Select(weekGroup =>
                    {
                        var weekRecords = weekGroup.ToList();
                        var firstRecord = weekRecords.First();

                        // Aggregate data for the week
                        var aggregatedData = new AggregatedWeekData
                        {
                            Date = weekGroup.Key,
                            MedianLocalSalary = weekRecords.Average(r => r.Data.MedianLocalSalary),
                            AverageLocalSalary = weekRecords.Average(r => r.Data.AverageLocalSalary),
                            MinLocalSalary = weekRecords.Min(r => r.Data.MinLocalSalary),
                            MaxLocalSalary = weekRecords.Max(r => r.Data.MaxLocalSalary),
                            TotalSalaryCount = weekRecords.Sum(r => r.Data.TotalSalaryCount),
                            MedianLocalSalaryByGrade = AggregateMedianByGrade(weekRecords)
                        };

                        return new HistoricalDataPoint(aggregatedData);
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                return new HistoricalDataByTemplate(
                    template.Id,
                    template.Name,
                    template.ProfessionIds ?? new List<long>(),
                    dataPoints);
            })
            .ToList();

        return new GetSalariesHistoricalChartResponse(
            groupedByTemplate,
            from,
            to);
    }

    private static DateTimeOffset GetStartOfWeek(DateTimeOffset date)
    {
        // Get the start of the week (Monday)
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    private static Dictionary<Domain.Enums.GradeGroup, double> AggregateMedianByGrade(
        List<Domain.Entities.HistoricalRecords.SalariesHistoricalDataRecord> records)
    {
        var allGrades = records
            .SelectMany(r => r.Data.MedianLocalSalaryByGrade ?? new Dictionary<Domain.Enums.GradeGroup, double>())
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Average(kvp => kvp.Value));

        return allGrades;
    }
}