using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using Domain.Enums;
using Domain.Extensions;
using Domain.Validation;
using Domain.ValueObjects;

namespace Domain.Entities.Users;

public class User : BaseModel, IHasDeletedAt
{
    public const int NameLength = 150;

    protected User()
    {
    }

    public User(
        string email,
        string firstName,
        string lastName,
        params Role[] roles)
    {
        Email = email.ThrowIfNullOrEmpty(nameof(email));
        FirstName = firstName.ThrowIfNullOrEmpty(nameof(firstName));
        LastName = lastName.ThrowIfNullOrEmpty(nameof(lastName));
        EmailConfirmed = false;

        UserRoles = new List<UserRole>();

        foreach (var role in roles)
        {
            UserRoles.Add(new UserRole(role, this));
        }
    }

    public User(
        CurrentUser currentUser)
        : this(
            currentUser.Email,
            currentUser.FirstName ?? currentUser.Email,
            currentUser.LastName ?? "-",
            currentUser.Roles.ToArray())
    {
        IdentityId = currentUser.Id;
        EmailConfirmed = true;
    }

    [Required]
    [StringLength(NameLength)]
    public string Email { get; protected set; }

    [Required]
    [StringLength(NameLength)]
    public string FirstName { get; protected set; }

    [Required]
    [StringLength(NameLength)]
    public string LastName { get; protected set; }

    public string IdentityId { get; protected set; }

    public bool EmailConfirmed { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public DateTimeOffset? LastLoginAt { get; protected set; }

    [JsonIgnore]
    public virtual ICollection<UserRole> UserRoles { get; protected set; } = new List<UserRole>();

    [NotMapped]
    public IReadOnlyCollection<Role> Roles
        => UserRoles
            .CollectionOrEmpty()
            .Select(x => x.RoleId)
            .ToArray();

    [NotMapped]
    public string Fullname => $"{FirstName} {LastName}";

    public void ConfirmEmail()
    {
        if (EmailConfirmed)
        {
            throw new InvalidOperationException("The email was confirmed already");
        }

        EmailConfirmed = true;
    }

    public void SetIdentityId(CurrentUser currentUser)
    {
        currentUser.ThrowIfNull(nameof(currentUser));
        if (IdentityId != null)
        {
            throw new InvalidOperationException($"The user {Id} has identity Id");
        }

        IdentityId = currentUser.Id;
    }

    public void SetRoles(
        IReadOnlyCollection<Role> roles)
    {
        if (UserRoles.Any())
        {
            throw new InvalidOperationException($"The user Id:{Id} has roles");
        }

        foreach (var role in roles)
        {
            UserRoles.Add(new UserRole(role, this));
        }
    }

    public void AddRole(Role role)
    {
        UserRoles.ThrowIfNull(nameof(UserRoles));

        if (UserRoles.Any(x => x.RoleId == role))
        {
            throw new InvalidOperationException($"The user Id:{Id} has the role {role}");
        }

        UserRoles.Add(new UserRole(role, this));
    }

    public void SyncRoles(IReadOnlyCollection<Role> roles)
    {
        roles.ThrowIfNull(nameof(roles));
        UserRoles.ThrowIfNull(nameof(UserRoles));

        UserRoles.Clear();

        foreach (Role role in roles)
        {
            UserRoles.Add(new UserRole(role, this));
        }
    }

    public void RenewLastLoginTime()
    {
        LastLoginAt = DateTimeOffset.Now;
    }

    public void Delete()
    {
        if (!this.Active())
        {
            throw new InvalidOperationException($"The user Id:{Id} is deleted");
        }

        DeletedAt = DateTimeOffset.Now;
    }

    public void Restore()
    {
        if (this.Active())
        {
            throw new InvalidOperationException($"The user Id:{Id} is active");
        }

        DeletedAt = null;
    }

    public void Update(
        string firstName,
        string lastName)
    {
        firstName = firstName?.Trim();
        lastName = lastName?.Trim();

        if (string.IsNullOrEmpty(firstName) && firstName != FirstName)
        {
            FirstName = firstName;
        }

        if (string.IsNullOrEmpty(lastName) && lastName != LastName)
        {
            LastName = lastName;
        }
    }
}