using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        var to = DateTimeOffset.Now;
        var from = to.AddMonths(-12);

        if (currentUser != null)
        {
            var userSalariesForLastYear = await _context.Salaries
                .GetUserRelevantSalariesAsync(
                    currentUser.Id,
                    cancellationToken);

            shouldAddOwnSalary = !userSalariesForLastYear.Any();
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
                CacheKey,
                async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                    return await _context.SalariesSurveyReplies
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
                        .Select(x => new SurveyDatabaseData
                        {
                            ExpectationReply = x.ExpectationReply,
                            UsefulnessReply = x.UsefulnessReply,
                            CreatedAt = x.CreatedAt,
                            LastSalaryOrNull = x.LastSalaryOrNull != null
                                ? new SurveyDatabaseData.UserLastSalaryData
                                {
                                    CompanyType = x.LastSalaryOrNull.Company,
                                    Grade = x.LastSalaryOrNull.Grade,
                                    CreatedAt = x.LastSalaryOrNull.CreatedAt,
                                }
                                : null,
                        })
                        .AsNoTracking()
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