using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Entities.Users;
using Domain.Enums;

namespace Domain.Services.Users;

public record CreateUserRequest
{
    [Required]
    [StringLength(User.NameLength)]
    public string Email { get; init; }

    [Required]
    [StringLength(User.NameLength)]
    public string FirstName { get; init; }

    [Required]
    [StringLength(User.NameLength)]
    public string LastName { get; init; }

    [CollectionNotEmptyBase]
    public IReadOnlyCollection<Role> Roles { get; init; }
}