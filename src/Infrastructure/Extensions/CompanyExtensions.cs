using Domain.Entities.Companies;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class CompanyExtensions
{
    public static async Task<Company> GetCompanyByIdentifierOrNullAsync(
        this IQueryable<Company> query,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        if (Guid.TryParse(identifier, out var guid))
        {
            return await query
                .FirstOrDefaultAsync(
                    c => c.Id == guid,
                    cancellationToken);
        }

        return await query
            .FirstOrDefaultAsync(
                c => c.Slug == identifier,
                cancellationToken);
    }
}