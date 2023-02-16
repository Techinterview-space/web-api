using System.Collections.Generic;
using Domain.Consumers.Contract.Enums;

namespace Domain.Consumers.Contract.Messages;

public interface IHasUserData
{
    public long Id { get; }

    public string Email { get; }

    public string FirstName { get; }

    public string LastName { get; }

    public ICollection<SharedUserRole> Roles { get; }
}