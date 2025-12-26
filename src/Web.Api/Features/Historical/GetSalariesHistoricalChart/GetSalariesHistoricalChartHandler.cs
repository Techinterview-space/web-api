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
                var dataPoints = group
                    .OrderBy(x => x.Date)
                    .Select(record => new HistoricalDataPoint(record))
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
}