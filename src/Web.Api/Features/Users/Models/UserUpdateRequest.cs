using System.ComponentModel.DataAnnotations;
using Domain.Entities.Users;

namespace Web.Api.Features.Users.Models;

public record UserUpdateRequest : UserUpdateRolesRequest
{
    [StringLength(User.NameLength)]
    public string FirstName { get; init; }

    [StringLength(User.NameLength)]
    public string LastName { get; init; }
}