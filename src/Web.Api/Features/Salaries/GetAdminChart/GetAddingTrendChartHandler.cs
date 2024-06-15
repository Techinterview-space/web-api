using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Salaries.GetAdminChart;

public class GetAddingTrendChartHandler
    : IRequestHandler<GetAddingTrendChartQuery, GetAddingTrendChartResponse>
{
    private readonly DatabaseContext _context;

    public GetAddingTrendChartHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<GetAddingTrendChartResponse> Handle(
        GetAddingTrendChartQuery request,
        CancellationToken cancellationToken)
    {
        var currentDay = DateTime.UtcNow.Date;
        var fifteenDaysAgo = currentDay.AddDays(-20);

        var salaries = await _context.Salaries
            .When(request.Grade.HasValue, x => x.Grade == request.Grade)
            .When(
                request.ProfessionsToInclude.Count > 0,
                x =>
                    x.ProfessionId.HasValue &&
                    request.ProfessionsToInclude.Contains(x.ProfessionId.Value))
            .When(request.Cities.Count > 0, x =>
                x.City.HasValue &&
                request.Cities.Contains(x.City.Value))
            .Select(x => new
            {
                x.Id,
                x.CreatedAt
            })
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var response = new GetAddingTrendChartResponse();

        var salariesFifteenDaysAgoAdded = salaries
            .Where(x => x.CreatedAt >= fifteenDaysAgo)
            .ToList();

        var daysSplitter = new DateTimeRoundedRangeSplitter(fifteenDaysAgo, currentDay, 1440);

        foreach (var (start, end) in daysSplitter.ToList())
        {
            var count = salariesFifteenDaysAgoAdded.Count(x =>
                x.CreatedAt >= start &&
                (x.CreatedAt < end || x.CreatedAt == end));

            response.Items.Add(new GetAddingTrendChartResponse.AdminChartItem(count, start));
            response.Labels.Add(start.ToString(GetAddingTrendChartResponse.DateTimeFormat));
        }

        return response;
    }
}