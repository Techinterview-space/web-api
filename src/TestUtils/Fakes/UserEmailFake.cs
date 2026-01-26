using System;
using Domain.Entities.Users;

namespace TestUtils.Fakes;

public class UserEmailFake : UserEmail
{
    public UserEmailFake(
        UserEmailType type,
        User user,
        DateTimeOffset? createdAt = null,
        string subject = "Test email subject")
        : base(
            subject, type, user)
    {
        if (createdAt.HasValue)
        {
            CreatedAt = createdAt.Value;
        }
    }
}