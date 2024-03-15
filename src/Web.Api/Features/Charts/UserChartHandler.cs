using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Domain.Salaries;
using Domain.Services.Salaries;
using Domain.Tools;
using Domain.ValueObjects.Dates;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Controllers.Salaries;
using TechInterviewer.Controllers.Salaries.Charts;

namespace TechInterviewer.Features.Charts;

public class UserChartHandler
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public UserChartHandler(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    public async Task<SalariesChartResponse> Handle(
        ISalariesChartQueryParams request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();

        var userSalariesForLastYear = new List<UserSalary>();

        var salariesQuery = new SalariesForChartQuery(_context, request);

        if (currentUser != null)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);

            userSalariesForLastYear = await _context.Salaries
                .Where(x => x.UserId == user.Id)
                .Where(x => x.Year == salariesQuery.CurrentQuarter.Year || x.Year == salariesQuery.CurrentQuarter.Year - 1)
                .AsNoTracking()
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Quarter)
                .ToListAsync(cancellationToken);
        }

        var query = salariesQuery.ToQueryable();
        if (currentUser == null || !userSalariesForLastYear.Any())
        {
            var salaryValues = await query
                .Where(x => x.Company == CompanyType.Local)
                .Select(x => x.Value)
                .ToListAsync(cancellationToken);

            var totalCount = await query.CountAsync(cancellationToken);
            return SalariesChartResponse.RequireOwnSalary(
                salaryValues,
                totalCount,
                true,
                currentUser is not null);
        }

        var salaries = await query.ToListAsync(cancellationToken);

        return new SalariesChartResponse(
            salaries,
            new UserSalaryAdminDto(userSalariesForLastYear.First()),
            salariesQuery.SalaryAddedEdge,
            DateTimeOffset.Now,
            salaries.Count);
    }

    private static IQueryable<UserSalary> ApplyFilters(
        IQueryable<UserSalary> query,
        SalariesChartQueryParams request)
    {
        var professionsToInclude = request.ProfessionsToInclude;
        if (professionsToInclude.Count > 0 && professionsToInclude.Contains((long)UserProfessionEnum.Developer))
        {
            professionsToInclude.AddIfDoesNotExist(
                (long)UserProfessionEnum.BackendDeveloper,
                (long)UserProfessionEnum.FrontendDeveloper,
                (long)UserProfessionEnum.FullstackDeveloper,
                (long)UserProfessionEnum.MobileDeveloper,
                (long)UserProfessionEnum.IosDeveloper,
                (long)UserProfessionEnum.AndroidDeveloper,
                (long)UserProfessionEnum.GameDeveloper);
        }

        return query
            .When(request.Grade.HasValue, x => x.Grade == request.Grade.Value)
            .When(professionsToInclude.Count > 0, x => x.ProfessionId != null && professionsToInclude.Contains(x.ProfessionId.Value))
            .FilterByCitiesIfNecessary(request.Cities);
    }
}