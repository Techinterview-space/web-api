using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Database;
using Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace TechInterviewer.Features.Salaries.GetAdminChart;

public class GetAdminChartHandler : IRequestHandler<GetAdminChartQuery, AdminChartResponse>
{
    private readonly DatabaseContext _context;

    public GetAdminChartHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<AdminChartResponse> Handle(
        GetAdminChartQuery request,
        CancellationToken cancellationToken)
    {
        var currentDay = DateTime.UtcNow.Date;
        var fifteenDaysAgo = currentDay.AddDays(-20);

        var usersCount = await _context.Users
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var salaries = await _context.Salaries
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.CreatedAt
            })
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var usersWhoLeftSalaries = salaries.GroupBy(x => x.UserId).Count();

        var response = new AdminChartResponse
        {
            SalariesPerUser = (double)salaries.Count / usersWhoLeftSalaries,
            UsersWhoLeftSalary = usersWhoLeftSalaries,
            AllUsersCount = usersCount,
        };

        var salariesFifteenDaysAgoAdded = salaries
            .Where(x => x.CreatedAt >= fifteenDaysAgo)
            .ToList();

        var daysSplitter = new DateTimeRoundedRangeSplitter(fifteenDaysAgo, currentDay, 1440);

        foreach (var (start, end) in daysSplitter.ToList())
        {
            var count = salariesFifteenDaysAgoAdded.Count(x =>
                x.CreatedAt >= start &&
                (x.CreatedAt < end || x.CreatedAt == end));

            response.Items.Add(new AdminChartResponse.AdminChartItem(count, start));
            response.Labels.Add(start.ToString(AdminChartResponse.DateTimeFormat));
        }

        return response;
    }
}