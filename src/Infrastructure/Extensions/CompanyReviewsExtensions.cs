using Domain.Entities.Companies;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class CompanyReviewsExtensions
{
    public static async Task<bool> HasRecentReviewAsync(
        this IQueryable<CompanyReview> reviews,
        Guid companyId,
        long userId,
        CancellationToken cancellationToken)
    {
        var threeMonthAgo = DateTimeOffset.UtcNow.AddMonths(-3);
        return await reviews
            .AnyAsync(
                cr =>
                    cr.CompanyId == companyId &&
                    cr.UserId == userId &&
                    cr.OutdatedAt == null &&
                    cr.CreatedAt >= threeMonthAgo,
                cancellationToken: cancellationToken);
    }
}