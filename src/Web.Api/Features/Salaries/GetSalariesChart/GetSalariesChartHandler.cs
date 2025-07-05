using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Labels.Models;
using Web.Api.Features.Salaries.GetSalariesChart.Charts;
using Web.Api.Features.Salaries.Models;
using Web.Api.Features.Surveys.Services;

namespace Web.Api.Features.Salaries.GetSalariesChart
{
    public class GetSalariesChartHandler : Infrastructure.Services.Mediator.IRequestHandler<GetSalariesChartQuery, SalariesChartResponse>
    {
        private static readonly List<DeveloperGrade> _grades = new List<DeveloperGrade>
        {
            DeveloperGrade.Junior,
            DeveloperGrade.Middle,
            DeveloperGrade.Senior,
            DeveloperGrade.Lead,
        };

        private readonly IAuthorization _auth;
        private readonly DatabaseContext _context;
        private readonly ICurrencyService _currencyService;

        public GetSalariesChartHandler(
            IAuthorization auth,
            DatabaseContext context,
            ICurrencyService currencyService)
        {
            _auth = auth;
            _context = context;
            _currencyService = currencyService;
        }

        public async Task<SalariesChartResponse> Handle(
            ISalariesChartQueryParams request,
            CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserOrNullAsync(cancellationToken);

            var userSalariesForLastYear = new List<UserSalary>();

            var now = DateTimeOffset.Now;
            var salariesQuery = new SalariesForChartQuery(
                _context,
                request,
                now);

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
            if (currentUser == null || userSalariesForLastYear.Count == 0)
            {
                var salaryValues = await query
                    .Select(x => new { x.Company, x.Value })
                    .ToListAsync(cancellationToken);

                var totalCount = await query.CountAsync(cancellationToken);
                return SalariesChartResponse.RequireOwnSalary(
                    salaryValues.Select(x => (x.Company, x.Value)).ToList(),
                    totalCount,
                    true,
                    currentUser is not null);
            }

            var salaries = await query.ToListAsync(cancellationToken);
            var hasSurveyRecentReply = await new SalariesSurveyUserService(_context)
                .HasFilledSurveyAsync(currentUser, cancellationToken);

            var currencies = await GetCurrenciesAsync(
                request,
                cancellationToken);

            // Fetch skills and industries data for chart data creation
            var skills = await _context.Skills
                .Select(x => new LabelEntityDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    HexColor = x.HexColor,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var industries = await _context.WorkIndustries
                .Select(x => new LabelEntityDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    HexColor = x.HexColor,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var salariesSkillsChartData = new SalariesSkillsChartData(salaries, skills);
            var workIndustriesChartData = new WorkIndustriesChartData(salaries, industries);
            var citiesDoughnutChartData = new CitiesDoughnutChartData(salaries);

            // Fetch professions data for professions distribution chart
            var professions = await _context.Professions
                .Select(x => new LabelEntityDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    HexColor = x.HexColor,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Create new chart data structures
            var localSalaries = salaries
                .Where(x => x.Company == CompanyType.Local)
                .ToList();

            var remoteSalaries = salaries
                .Where(x => x.Company == CompanyType.Foreign)
                .ToList();

            var gradesMinMaxChartData = CreateGradesMinMaxChartData(salaries);
            var professionsDistributionChartData = CreateProfessionsDistributionChartData(
                localSalaries,
                remoteSalaries,
                professions);

            var peopleByGenderChartData = new PeopleByGenderChartData(
                CreateGenderDistributionData(localSalaries),
                CreateGenderDistributionData(remoteSalaries));

            return new SalariesChartResponse(
                salaries,
                new UserSalaryAdminDto(userSalariesForLastYear.First()),
                hasSurveyRecentReply,
                salariesQuery.From,
                salariesQuery.To,
                currencies,
                salariesSkillsChartData,
                workIndustriesChartData,
                citiesDoughnutChartData,
                gradesMinMaxChartData,
                professionsDistributionChartData,
                peopleByGenderChartData);
        }

        public Task<SalariesChartResponse> Handle(
            GetSalariesChartQuery request,
            CancellationToken cancellationToken)
        {
            return Handle((ISalariesChartQueryParams)request, cancellationToken);
        }

        private static GradesMinMaxChartData CreateGradesMinMaxChartData(List<UserSalaryDto> salaries)
        {
            var localSalaries = salaries
                .Where(x => x.Company == CompanyType.Local)
                .TakeMiddleCollection(10, 10);

            var remoteSalaries = salaries
                .Where(x => x.Company == CompanyType.Foreign)
                .TakeMiddleCollection(10, 10);

            var localData = _grades
                .Select(grade => CreateGradeBoxPlotData(localSalaries, grade))
                .ToList();

            var remoteData = _grades
                .Select(grade => CreateGradeBoxPlotData(remoteSalaries, grade))
                .ToList();

            return new GradesMinMaxChartData(localData, remoteData);
        }

        private static GradeBoxPlotData CreateGradeBoxPlotData(List<UserSalaryDto> salaries, DeveloperGrade grade)
        {
            var gradeSalaries = salaries
                .Where(x => x.Grade == grade)
                .Select(x => x.Value)
                .OrderBy(x => x)
                .ToList();

            if (gradeSalaries.Count == 0)
            {
                return new GradeBoxPlotData(grade, 0, 0, 0, 0, 0, 0, new List<double>());
            }

            var min = gradeSalaries.First();
            var max = gradeSalaries.Last();
            var median = gradeSalaries.Median();
            var mean = gradeSalaries.Average();

            // Calculate quartiles properly
            var q1 = gradeSalaries.GetPercentileValue(25);
            var q3 = gradeSalaries.GetPercentileValue(75);

            return new GradeBoxPlotData(grade, min, q1, median, q3, max, mean, gradeSalaries);
        }

        private static ProfessionsDistributionChartData CreateProfessionsDistributionChartData(
            List<UserSalaryDto> localSalaries,
            List<UserSalaryDto> remoteSalaries,
            List<LabelEntityDto> professions)
        {
            var localData = CreateProfessionDistributionData(localSalaries, professions);
            var remoteData = CreateProfessionDistributionData(remoteSalaries, professions);

            return new ProfessionsDistributionChartData(localData, remoteData);
        }

        private static ProfessionDistributionData CreateProfessionDistributionData(
            List<UserSalaryDto> salaries,
            List<LabelEntityDto> professions)
        {
            if (salaries.Count == 0)
            {
                return new ProfessionDistributionData();
            }

            var professionCounts = salaries
                .GroupBy(x => x.ProfessionId)
                .Select(g => new
                {
                    ProfessionId = g.Key,
                    Count = g.Count(),
                    Percentage = (double)g.Count() / salaries.Count * 100
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var items = professionCounts
                .Where(x => x.ProfessionId.HasValue)
                .Select(x =>
                {
                    var profession = professions.FirstOrDefault(p => p.Id == x.ProfessionId.Value);
                    return profession != null
                        ? new ProfessionDistributionItem(profession, x.Count, x.Percentage)
                        : null;
                })
                .Where(x => x != null)
                .ToList();

            var otherCount = professionCounts.Where(x => !x.ProfessionId.HasValue).Sum(x => x.Count);
            var otherPercentage = otherCount > 0 ? (double)otherCount / salaries.Count * 100 : 0;

            return new ProfessionDistributionData(items, otherCount, otherPercentage, salaries.Count);
        }

        private static GenderDistributionData CreateGenderDistributionData(
            List<UserSalaryDto> salaries)
        {
            if (salaries.Count == 0)
            {
                return new GenderDistributionData();
            }

            var genderCounts = salaries
                .Where(x => x.Gender.HasValue && x.Gender != Domain.Enums.Gender.Undefined)
                .GroupBy(x => x.Gender.Value)
                .Select(g => new GenderDistributionItem(
                    g.Key,
                    g.Count(),
                    (double)g.Count() / salaries.Count * 100))
                .ToList();

            var noGenderCount = salaries.Count(x => !x.Gender.HasValue || x.Gender == Domain.Enums.Gender.Undefined);
            var noGenderPercentage = noGenderCount > 0 ? (double)noGenderCount / salaries.Count * 100 : 0;

            return new GenderDistributionData(genderCounts, noGenderCount, noGenderPercentage, salaries.Count);
        }

        private async Task<List<CurrencyContent>> GetCurrenciesAsync(
            ISalariesChartQueryParams request,
            CancellationToken cancellationToken)
        {
            if (request.SalarySourceTypes.Count > 0)
            {
                if (request.SalarySourceTypes.First() is SalarySourceType.KolesaDevelopersCsv2022)
                {
                    var date = new DateTime(2021, 12, 31);
                    return
                    [
                        new CurrencyContent(
                            1,
                            Currency.KZT,
                            date),

                        new CurrencyContent(
                            434,
                            Currency.USD,
                            date),

                        new CurrencyContent(
                            491,
                            Currency.EUR,
                            date)

                    ];
                }

                if (request.SalarySourceTypes.First() is SalarySourceType.KolesaDataAnalystCsv2024)
                {
                    var date = new DateTime(2024, 10, 05);
                    return
                    [
                        new CurrencyContent(
                            1,
                            Currency.KZT,
                            date),

                        new CurrencyContent(
                            485,
                            Currency.USD,
                            date),
                    ];
                }
            }

            return await _currencyService
                .GetAllCurrenciesAsync(cancellationToken);
        }
    }
}