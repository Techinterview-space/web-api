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

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public class GetSurveyHistoricalChartHandler
    : IRequestHandler<GetSurveyHistoricalChartQuery, GetSurveyHistoricalChartResponse>
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public GetSurveyHistoricalChartHandler(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
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

        var surveyReplies = await _context.SalariesSurveyReplies
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
            .ToListAsync(cancellationToken);

        return new GetSurveyHistoricalChartResponse(
            surveyReplies,
            from,
            to);
    }
}