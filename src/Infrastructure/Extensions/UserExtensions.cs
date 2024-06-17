using Domain.Entities.Salaries;
using Domain.Entities.Users;
using Domain.ValueObjects.Dates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class UserExtensions
{
    public static Task<List<UserSalary>> GetUserRelevantSalariesAsync(
        this IQueryable<UserSalary> salaries,
        long userId,
        CancellationToken cancellationToken)
    {
        var currentQuarter = DateQuarter.Current;
        return salaries
            .Where(x => x.UserId == userId)
            .Where(x => x.Year == currentQuarter.Year || x.Year == currentQuarter.Year - 1)
            .AsNoTracking()
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Quarter)
            .ToListAsync(cancellationToken);
    }
}