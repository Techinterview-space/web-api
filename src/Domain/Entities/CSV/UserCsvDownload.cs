using System;
using Domain.Entities.Users;
using Domain.Enums;

namespace Domain.Entities.CSV;

public class UserCsvDownload : HasDatesBase, IHasIdBase<Guid>
{
    public const int CountOfHoursToAllowDownload = 48;

    public UserCsvDownload()
    {
    }

    public UserCsvDownload(
        User user)
    {
        UserId = user.Id;
    }

    public Guid Id { get; protected set; }

    public long UserId { get; protected set; }

    public virtual User User { get; protected set; }

    public bool AllowsDownload(
        User currentUser)
    {
        if (currentUser.Has(Role.Admin))
        {
            return true;
        }

        return CreatedAt.AddHours(CountOfHoursToAllowDownload) < DateTime.UtcNow;
    }
}