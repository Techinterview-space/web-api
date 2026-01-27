using System.Linq.Expressions;
using Domain.Entities.Users;
using Domain.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Extensions;

public static class UserQueryExtensions
{
    public static async Task<User> ByEmailOrIdentityIdOrNullAsync(
        this IQueryable<User> query,
        string email,
        string identityId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentNullException(paramName: nameof(email));
        }

        var emailUpper = email.ToUpperInvariant();
        Expression<Func<User, bool>> expression = x => x.Email.ToUpper() == emailUpper;
        if (!string.IsNullOrEmpty(identityId))
        {
            expression = expression.Or(x => x.IdentityId == identityId);
        }

        return await query
            .FirstOrDefaultAsync(expression, cancellationToken: cancellationToken);
    }
}