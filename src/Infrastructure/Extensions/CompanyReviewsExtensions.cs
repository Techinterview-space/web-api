using Domain.Entities.Companies;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class CompanyReviewsExtensions
{
    public static Task<bool> HasRecentReviewAsync(
        this IQueryable<CompanyReview> reviews,
        Guid? companyId,
        long userId,
        CancellationToken cancellationToken)
    {
        return HasRecentReviewAsync(
            reviews: reviews,
            companyId: companyId,
            userId: userId,
            countOfMonthsForEdge: 3,
            cancellationToken: cancellationToken);
    }

    public static async Task<bool> HasRecentReviewAsync(
        this IQueryable<CompanyReview> reviews,
        Guid? companyId,
        long userId,
        int countOfMonthsForEdge,
        CancellationToken cancellationToken)
    {
        var edge = DateTimeOffset.UtcNow.AddMonths(-countOfMonthsForEdge);
        return await reviews
            .When(companyId.HasValue, x => x.CompanyId == companyId)
            .AnyAsync(
                cr =>
                    cr.UserId == userId &&
                    cr.OutdatedAt == null &&
                    cr.CreatedAt >= edge,
                cancellationToken: cancellationToken);
    }
}