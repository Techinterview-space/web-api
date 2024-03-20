using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Extensions;

public static class UserQueryExtensions
{
    public static async Task<User> ByEmailOrNullAsync(
        this IQueryable<User> query,
        string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentNullException(paramName: nameof(email));
        }

        var emailUpper = email.ToUpperInvariant();
        return await query
            .FirstOrDefaultAsync(x => x.Email.ToUpper() == emailUpper);
    }
}