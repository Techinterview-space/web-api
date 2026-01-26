using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using Domain.Entities.Companies;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Extensions;
using Domain.Totp;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Domain.ValueObjects;
using OtpNet;

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

        UniqueToken = Guid.NewGuid().ToString("N");
        LastLoginAt = DateTimeOffset.Now;
    }

    public User(
        CurrentUser currentUser)
        : this(
            currentUser.Email,
            currentUser.FirstName ?? currentUser.Email,
            !string.IsNullOrEmpty(currentUser.LastName) ? currentUser.LastName : "-",
            currentUser.Roles.ToArray())
    {
        IdentityId = currentUser.UserId;
        EmailConfirmed = currentUser.IsGoogleAuth() || currentUser.IsGithubAuth();
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

    public string ProfilePicture { get; protected set; }

    public bool EmailConfirmed { get; protected set; }

    public string UniqueToken { get; protected set; }

    public bool UnsubscribeMeFromAll { get; protected set; }

    public string TotpSecret { get; protected set; }

    public DateTimeOffset? TotpVerifiedAt { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public DateTimeOffset? LastLoginAt { get; protected set; }

    public string PasswordHash { get; protected set; }

    public string PasswordResetToken { get; protected set; }

    public DateTimeOffset? PasswordResetTokenExpiresAt { get; protected set; }

    public string EmailVerificationToken { get; protected set; }

    public DateTimeOffset? EmailVerificationTokenExpiresAt { get; protected set; }

    public int FailedLoginAttempts { get; protected set; }

    public DateTimeOffset? LockedUntil { get; protected set; }

    public virtual List<UserRole> UserRoles { get; protected set; } = new ();

    public virtual List<UserSalary> Salaries { get; protected set; } = new ();

    public virtual List<CompanyReview> Reviews { get; protected set; } = new ();

    public virtual List<CompanyReviewVote> Votes { get; protected set; }

    public virtual List<UserEmail> Emails { get; protected set; }

    public string GetFullname()
    {
        return $"{FirstName} {LastName}";
    }

    public bool IsMfaEnabled() => TotpSecret != null;

    public List<Role> GetRoles()
    {
        return UserRoles
            .CollectionOrEmpty()
            .Select(x => x.RoleId)
            .ToList();
    }

    public void ConfirmEmail()
    {
        if (EmailConfirmed)
        {
            throw new InvalidOperationException("The email was confirmed already");
        }

        EmailConfirmed = true;
    }

    public bool CheckIdentity(
        CurrentUser currentUser)
    {
        if (string.IsNullOrWhiteSpace(IdentityId))
        {
            return true;
        }

        var userId = currentUser.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var result = IdentityId.Equals(userId, StringComparison.InvariantCultureIgnoreCase);
        if (result)
        {
            return true;
        }

        if (Has(Role.Admin))
        {
            return false;
        }

        var isLoggedAsSocial = currentUser.IsGoogleAuth() || currentUser.IsGithubAuth();
        if (isLoggedAsSocial)
        {
            EmailConfirmed = true;
        }

        return isLoggedAsSocial;
    }

    public bool UpdateData(
        CurrentUser currentUser)
    {
        currentUser.ThrowIfNull(nameof(currentUser));

        var needUpdates = false;
        if (IdentityId == null)
        {
            IdentityId = currentUser.UserId;
            needUpdates = true;
        }
        else if (
            IdentityId == currentUser.UserId &&
            Email != currentUser.Email &&
            (currentUser.IsGoogleAuth() || currentUser.IsGithubAuth()))
        {
            Email = currentUser.Email;
        }

        if (ProfilePicture == null ||
            ProfilePicture != currentUser.ProfilePicture)
        {
            ProfilePicture = currentUser.ProfilePicture;
            needUpdates = true;
        }

        if (GetRoles().Count == 0 && currentUser.Roles.Count > 0)
        {
            foreach (var role in currentUser.Roles)
            {
                UserRoles.Add(new UserRole(role, this));
            }

            needUpdates = true;
        }

        if (!EmailConfirmed &&
            (currentUser.IsGoogleAuth() || currentUser.IsGithubAuth()))
        {
            EmailConfirmed = true;
            needUpdates = true;
        }

        return needUpdates;
    }

    public void AddRole(
        Role role)
    {
        UserRoles.ThrowIfNull(nameof(UserRoles));

        if (UserRoles.Any(x => x.RoleId == role))
        {
            throw new InvalidOperationException($"The user Id:{Id} has the role {role}");
        }

        UserRoles.Add(new UserRole(role, this));
    }

    public void SyncRoles(
        IReadOnlyCollection<Role> roles)
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

    public void GenerateTotpSecretKey()
    {
        if (TotpSecret != null)
        {
            throw new InvalidOperationException($"The user Id:{Id} has TotpSecretKey");
        }

        TotpSecret = TotpSecretKey.Random().KeyAsBase32;
    }

    public void DisableTotp()
    {
        if (TotpSecret == null)
        {
            throw new InvalidOperationException($"The user Id:{Id} has no TotpSecretKey");
        }

        TotpSecret = null;
    }

    public TotpSecretKey GetTotpSecretKey()
    {
        if (TotpSecret == null)
        {
            throw new InvalidOperationException($"The user Id:{Id} has no TotpSecretKey");
        }

        return new TotpSecretKey(TotpSecret);
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

    public bool Has(Role role)
    {
        return UserRoles.Any(x => x.RoleId == role);
    }

    public void HasAnyOrFail(
        params Role[] roles) =>
        HasAnyOrFail(roles as IReadOnlyCollection<Role>);

    public void HasAnyOrFail(
        IReadOnlyCollection<Role> roles)
    {
        roles.ThrowIfNullOrEmpty(nameof(roles));

        if (roles.Any(Has))
        {
            return;
        }

        throw new NoPermissionsException("Current user has no permission to do this operation");
    }

    public void HasOrFail(
        Role role,
        string messageOrNull = null)
    {
        if (Has(role))
        {
            return;
        }

        throw new NoPermissionsException(messageOrNull ?? "Current user has no permission to do this operation");
    }

    public User InactiveOrFail()
    {
        if (!this.Active())
        {
            return this;
        }

        throw new InvalidOperationException($"The user Id:{Id} is active");
    }

    public bool TotpVerificationExpired()
    {
        return TotpVerifiedAt == null ||
               TotpVerifiedAt.Value.AddHours(6) < DateTimeOffset.Now;
    }

    public bool VerifyTotp(
        string totpCode)
    {
        var totp = new OtpNet.Totp(GetTotpSecretKey().KeyAsBytes);
        var result = totp.VerifyTotp(
            totpCode,
            out _,
            new VerificationWindow(previous: 1, future: 1));

        if (result)
        {
            TotpVerifiedAt = DateTimeOffset.Now;
        }

        return result;
    }

    public bool IsGoogleAuth()
    {
        return IdentityId != null &&
               IdentityId.StartsWith(CurrentUser.GoogleOAuth2Prefix);
    }

    public bool IsGithubAuth()
    {
        return IdentityId != null &&
               IdentityId.StartsWith(CurrentUser.GithubPrefix);
    }

    public bool IsAuth0Auth()
    {
        return IdentityId != null &&
               IdentityId.StartsWith(CurrentUser.Auth0Prefix);
    }

    public bool GenerateNewEmailUnsubscribeTokenIfNecessary()
    {
        if (UniqueToken != null)
        {
            return false;
        }

        UniqueToken = Guid.NewGuid().ToString("N");
        return true;
    }

    public void UnsubscribeFromAll()
    {
        UnsubscribeMeFromAll = true;
    }

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void SetEmailVerificationToken(string token, TimeSpan validity)
    {
        EmailVerificationToken = token;
        EmailVerificationTokenExpiresAt = DateTimeOffset.UtcNow.Add(validity);
    }

    public void ClearEmailVerificationToken()
    {
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
    }

    public void SetPasswordResetToken(string token, TimeSpan validity)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = DateTimeOffset.UtcNow.Add(validity);
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
    }

    public void IncrementFailedLoginAttempts()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockedUntil = DateTimeOffset.UtcNow.AddMinutes(15);
        }
    }

    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public bool IsLockedOut => LockedUntil.HasValue && LockedUntil > DateTimeOffset.UtcNow;

    public bool IsLocalAuth()
    {
        return IdentityId != null &&
               IdentityId.StartsWith(CurrentUser.LocalPrefix);
    }

    public void SetIdentityId(string identityId)
    {
        IdentityId = identityId;
    }

    public void SetProfilePicture(string profilePicture)
    {
        ProfilePicture = profilePicture;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }
}