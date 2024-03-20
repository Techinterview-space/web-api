using Domain.Enums;
using Domain.Validation;

namespace Domain.Entities.Users;

public class UserRole
{
    protected UserRole()
    {
    }

    public UserRole(
        Role role,
        User user)
    {
        user.ThrowIfNull(nameof(user));

        RoleId = role;
        UserId = user.Id;
    }

    public Role RoleId { get; protected set; }

    public long UserId { get; protected set; }

    public virtual User User { get; protected set; }
}