using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Extensions;
using Infrastructure.Salaries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Historical.GetSalariesHistoricalChart;

public class GetSalariesHistoricalChartHandler
    : IRequestHandler<GetSalariesHistoricalChartQuery, GetSalariesHistoricalChartResponse>
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public GetSalariesHistoricalChartHandler(
        DatabaseContext context,
        IAuthorization auth)
    {
        _context = context;
        _auth = auth;
    }

    public async Task<GetSalariesHistoricalChartResponse> Handle(
        GetSalariesHistoricalChartQuery request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync(cancellationToken);

        var hasAuthentication = currentUser != null;
        var shouldAddOwnSalary = false;

        var to = request.To ?? DateTimeOffset.Now;
        var from = request.From ?? to.AddMonths(-12);

        var salariesQuery = new SalariesForChartQuery(
            _context,
            request,
            from,
            to);

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
            return GetSalariesHistoricalChartResponse.NoSalaryOrAuthorization(
                hasAuthentication,
                shouldAddOwnSalary,
                from,
                to);
        }

        var salaries = await salariesQuery
            .ToQueryable(x => new UserSalarySimpleDto
            {
                Value = x.Value,
                Company = x.Company,
                Grade = x.Grade,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return new GetSalariesHistoricalChartResponse(
            salaries,
            from,
            to,
            !request.Grade.HasValue);
    }
}