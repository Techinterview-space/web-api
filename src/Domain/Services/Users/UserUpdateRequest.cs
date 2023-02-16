using System.ComponentModel.DataAnnotations;
using Domain.Entities.Users;

namespace Domain.Services.Users;

public record UserUpdateRequest : UserUpdateRolesRequest
{
    [StringLength(User.NameLength)]
    public string FirstName { get; init; }

    [StringLength(User.NameLength)]
    public string LastName { get; init; }
}