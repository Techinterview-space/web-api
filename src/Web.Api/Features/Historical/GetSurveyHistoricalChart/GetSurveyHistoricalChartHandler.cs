using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Extensions;
using Domain.ValueObjects.Dates;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Web.Api.Features.Surveys.Services;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public class GetSurveyHistoricalChartHandler
    : IRequestHandler<GetSurveyHistoricalChartQuery, GetSurveyHistoricalChartResponse>
{
    private const string CacheKey = $"{nameof(GetSurveyHistoricalChartHandler)}_SurveyData";

    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;
    private readonly IMemoryCache _memoryCache;

    public GetSurveyHistoricalChartHandler(
        IAuthorization auth,
        DatabaseContext context,
        IMemoryCache memoryCache)
    {
        _auth = auth;
        _context = context;
        _memoryCache = memoryCache;
    }

    public async Task<GetSurveyHistoricalChartResponse> Handle(
        GetSurveyHistoricalChartQuery request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync(cancellationToken);

        var hasAuthentication = currentUser != null;
        var shouldAddOwnSalary = false;

        var to = request.To ?? DateTimeOffset.Now;
        var from = request.From ?? to.AddMonths(-12);

        if (currentUser != null)
        {
            var userSalariesForLastYear = await _context.Salaries
                .GetUserRelevantSalariesAsync(
                    currentUser.Id,
                    cancellationToken);

            shouldAddOwnSalary = userSalariesForLastYear.Count == 0;
        }

        if (currentUser is null || shouldAddOwnSalary)
        {
            return GetSurveyHistoricalChartResponse.NoSalaryOrAuthorization(
                hasAuthentication,
                shouldAddOwnSalary,
                from,
                to);
        }

        var lastSurvey = await new SalariesSurveyUserService(_context)
            .GetLastSurveyOrNullAsync(currentUser, cancellationToken);

        var surveyReplies = await _memoryCache
            .GetOrCreateAsync(
                CacheKey + request.GetKeyPostfix(),
                async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                    var query = _context.SalariesSurveyReplies
                        .Include(x => x.CreatedByUser)
                        .ThenInclude(x => x.Salaries)
                        .Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
                        .Select(x => new
                        {
                            x.ExpectationReply,
                            x.UsefulnessReply,
                            x.CreatedAt,
                            LastSalaryOrNull = x.CreatedByUser.Salaries
                                .OrderBy(s => s.CreatedAt)
                                .LastOrDefault()
                        })
                        .When(
                            request.Grade.HasValue,
                            x =>
                                x.LastSalaryOrNull != null &&
                                x.LastSalaryOrNull.Grade == request.Grade.Value)
                        .When(
                            request.Skills.Count > 0,
                            x =>
                                x.LastSalaryOrNull != null &&
                                x.LastSalaryOrNull.SkillId != null &&
                                request.Skills.Contains(x.LastSalaryOrNull.SkillId.Value))
                        .When(
                            request.ProfessionsToInclude.Count > 0,
                            x =>
                                x.LastSalaryOrNull != null &&
                                x.LastSalaryOrNull.ProfessionId != null &&
                                request.ProfessionsToInclude.Contains(x.LastSalaryOrNull.ProfessionId.Value))
                        .Select(x => new SurveyDatabaseData
                        {
                            ExpectationReply = x.ExpectationReply,
                            UsefulnessReply = x.UsefulnessReply,
                            CreatedAt = x.CreatedAt,
                            LastSalaryOrNull = x.LastSalaryOrNull != null
                                ? new SurveyDatabaseData.UserLastSalaryData
                                {
                                    City = x.LastSalaryOrNull.City,
                                    CompanyType = x.LastSalaryOrNull.Company,
                                    Grade = x.LastSalaryOrNull.Grade,
                                    CreatedAt = x.LastSalaryOrNull.CreatedAt,
                                }
                                : null,
                        })
                        .AsNoTracking();

                    if (request.Cities.Count != 0)
                    {
                        if (request.Cities.Count == 1 && request.Cities[0] == KazakhstanCity.Undefined)
                        {
                            query = query
                                .Where(s =>
                                    s.LastSalaryOrNull != null &&
                                    s.LastSalaryOrNull.City == null);
                        }

                        Expression<Func<SurveyDatabaseData, bool>> clause = s =>
                            s.LastSalaryOrNull.City != null &&
                            request.Cities.Contains(s.LastSalaryOrNull.City.Value);

                        if (request.Cities.Any(x => x == KazakhstanCity.Undefined))
                        {
                            clause = clause.Or(x =>
                                x.LastSalaryOrNull != null &&
                                x.LastSalaryOrNull.City == null);
                        }

                        query = query.Where(clause);
                    }

                    return await query
                        .OrderBy(x => x.CreatedAt)
                        .ToListAsync(cancellationToken);
                });

        return new GetSurveyHistoricalChartResponse(
            surveyReplies,
            from,
            to,
            lastSurvey?.CreatedAt);
    }
}