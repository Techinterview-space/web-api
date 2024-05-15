using Domain.Entities.Users;
using Domain.ValueObjects;
using Infrastructure.Database;
using Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authentication;

public class CurrentUserProvider
{
    private readonly DatabaseContext _context;
    private readonly CurrentUser _currentUser;

    public CurrentUserProvider(
        DatabaseContext context,
        CurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<User> GetOrCreateAsync()
    {
        var user = await _context.Users
            .Include(x => x.UserRoles)
            .Include(x => x.Salaries)
            .ByEmailOrNullAsync(_currentUser.Email);

        if (user == null)
        {
            var claimsUser = new User(_currentUser);
            user = await _context.AddEntityAsync(claimsUser);
            await _context.TrySaveChangesAsync();
            return user;
        }

        var save = false;
        if (!user.EmailConfirmed)
        {
            user.ConfirmEmail();
            save = true;
        }

        if (user.IdentityId == null)
        {
            user.SetIdentityId(_currentUser);
            save = true;
        }

        if (user.Roles.Count == 0 && _currentUser.Roles.Count > 0)
        {
            user.SetRoles(_currentUser.Roles);
            save = true;
        }

        if (save)
        {
            await _context.TrySaveChangesAsync();
        }

        return user;
    }
}