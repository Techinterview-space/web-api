using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Extensions;
using Domain.Services.Salaries;
using Domain.ValueObjects.Dates;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Controllers.Salaries;
using TechInterviewer.Controllers.Salaries.Charts;

namespace TechInterviewer.Features.Charts;

public class UserChartHandler(
    IAuthorization auth,
    DatabaseContext context)
{
    public async Task<SalariesChartResponse> Handle(
        SalariesChartQueryParams request,
        CancellationToken cancellationToken)
    {
        var currentUser = await auth.CurrentUserOrNullAsync();

        var userSalariesForLastYear = new List<UserSalary>();
        var currentQuarter = DateQuarter.Current;

        if (currentUser != null)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);

            userSalariesForLastYear = await context.Salaries
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
                    context.Salaries
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
                .When(request.HasAnyFilter, x => x.Company == CompanyType.Local)
                .Select(x => x.Value)
                .ToListAsync(cancellationToken);

            return SalariesChartResponse.RequireOwnSalary(
                salaryValues,
                request.HasAnyFilter);
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
            if (!professionsToInclude.Contains((long)UserProfessionEnum.BackendDeveloper))
            {
                professionsToInclude.Add((long)UserProfessionEnum.BackendDeveloper);
            }

            if (!professionsToInclude.Contains((long)UserProfessionEnum.FrontendDeveloper))
            {
                professionsToInclude.Add((long)UserProfessionEnum.FrontendDeveloper);
            }

            if (!professionsToInclude.Contains((long)UserProfessionEnum.FullstackDeveloper))
            {
                professionsToInclude.Add((long)UserProfessionEnum.FullstackDeveloper);
            }

            if (!professionsToInclude.Contains((long)UserProfessionEnum.MobileDeveloper))
            {
                professionsToInclude.Add((long)UserProfessionEnum.MobileDeveloper);
            }

            if (!professionsToInclude.Contains((long)UserProfessionEnum.IosDeveloper))
            {
                professionsToInclude.Add((long)UserProfessionEnum.IosDeveloper);
            }

            if (!professionsToInclude.Contains((long)UserProfessionEnum.AndroidDeveloper))
            {
                professionsToInclude.Add((long)UserProfessionEnum.AndroidDeveloper);
            }

            if (!professionsToInclude.Contains((long)UserProfessionEnum.GameDeveloper))
            {
                professionsToInclude.Add((long)UserProfessionEnum.GameDeveloper);
            }
        }

        return query
            .When(request.Grade.HasValue, x => x.Grade == request.Grade.Value)
            .When(professionsToInclude.Count > 0, x => x.ProfessionId != null && professionsToInclude.Contains(x.ProfessionId.Value))
            .FilterByCitiesIfNecessary(request.Cities);
    }
}