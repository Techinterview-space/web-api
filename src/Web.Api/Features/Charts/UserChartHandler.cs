using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Salaries;
using Domain.Extensions;
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
        SalariesChartQueryParams request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();

        var userSalariesForLastYear = new List<UserSalary>();
        var currentQuarter = DateQuarter.Current;

        if (currentUser != null)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);

            userSalariesForLastYear = await _context.Salaries
                .Where(x => x.UserId == user.Id)
                .Where(x => x.Year == currentQuarter.Year || x.Year == currentQuarter.Year - 1)
                .AsNoTracking()
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Quarter)
                .ToListAsync(cancellationToken);
        }

        var yearAgoGap = DateTimeOffset.Now.AddMonths(-6);

        var query =
            ApplyFilters(
                    _context.Salaries
                        .Where(x => x.UseInStats)
                        .Where(x => x.ProfessionId != (long)UserProfessionEnum.HrNonIt)
                        .Where(x => x.Year == currentQuarter.Year || x.Year == currentQuarter.Year - 1)
                        .Where(x => x.CreatedAt >= yearAgoGap),
                    request)
            .Select(x => new UserSalaryDto
            {
                Value = x.Value,
                Quarter = x.Quarter,
                Year = x.Year,
                Currency = x.Currency,
                Company = x.Company,
                Grade = x.Grade,
                City = x.City,
                Age = x.Age,
                YearOfStartingWork = x.YearOfStartingWork,
                Gender = x.Gender,
                SkillId = x.SkillId,
                WorkIndustryId = x.WorkIndustryId,
                ProfessionId = x.ProfessionId,
            })
            .OrderBy(x => x.Value)
            .AsNoTracking();

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
            yearAgoGap,
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