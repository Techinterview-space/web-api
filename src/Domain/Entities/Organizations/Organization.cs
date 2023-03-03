using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Domain.Entities.Employments;
using Domain.Entities.Interviews;
using Domain.Entities.Users;
using Domain.Enums;

namespace Domain.Entities.Organizations;

public class Organization : HasDatesBase, IHasIdBase<Guid>
{
    public const int NameLength = 200;
    public const int DescriptionFieldLength = 5000;

    protected Organization()
    {
    }

    public Organization(
        string name,
        string descriptionOrNull,
        User managerOrNull)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = descriptionOrNull;

        if (managerOrNull is not null)
        {
            ManagerId = managerOrNull?.Id;
            Users ??= new List<OrganizationUser>();
            Users.Add(new OrganizationUser(managerOrNull, this));
        }
    }

    public Guid Id { get; protected set; }

    [StringLength(NameLength)]
    [Required]
    public string Name { get; protected set; }

    [StringLength(DescriptionFieldLength)]
    public string Description { get; protected set; }

    public long? ManagerId { get; protected set; }

    public virtual User Manager { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public virtual ICollection<OrganizationUser> Users { get; protected set; } = new List<OrganizationUser>();

    public virtual ICollection<Candidate> Candidates { get; protected set; } = new List<Candidate>();

    public virtual ICollection<CandidateCard> CandidateCards { get; protected set; } = new List<CandidateCard>();

    public virtual ICollection<JoinToOrgInvitation> Invitations { get; protected set; } = new List<JoinToOrgInvitation>();

    public virtual ICollection<Interview> Interviews { get; protected set; } = new List<Interview>();

    public virtual ICollection<InterviewTemplate> InterviewTemplates { get; protected set; } = new List<InterviewTemplate>();

    [NotMapped]
    public bool Active => DeletedAt == null;

    public bool CouldBeModifiedBy(
        User user) =>
        user.Has(Role.Admin) || ManagerId == user.Id;

    public bool CouldBeOpenBy(
        User user) =>
        CouldBeModifiedBy(user) || user.OrganizationUsers.Any(x => x.OrganizationId == Id);

    public void Update(
        string name,
        string descriptionOrNull)
    {
        Name = name;
        Description = descriptionOrNull;
    }

    public bool IsManagerBy(User user)
        => ManagerId == user.Id;

    public bool HasUser(
        User user) =>
        HasUser(user.Id);

    public bool HasUser(
        long userId) =>
        Users.Any(x => x.UserId == userId);

    public void Archive()
    {
        if (DeletedAt != null)
        {
            throw new InvalidOperationException("Organization is already deleted");
        }

        DeletedAt = DateTimeOffset.Now;

        if (!Users.Any())
        {
            return;
        }

        foreach (var user in Users)
        {
            user.Delete();
        }
    }

    public Organization AttachUser(
        User user)
    {
        if (Users.Any(x => x.UserId == user.Id))
        {
            throw new InvalidOperationException($"User {user.Id} is already attached to the organization Id:{Id}");
        }

        Users.Add(new OrganizationUser(user, this));
        return this;
    }

    public void ChangeManager(
        User user)
    {
        if (!HasUser(user))
        {
            AttachUser(user);
        }

        ManagerId = user.Id;
    }

    public Organization ExcludeUser(
        long userId)
    {
        var orgUser = Users.FirstOrDefault(x => x.UserId == userId);
        if (orgUser is null)
        {
            throw new InvalidOperationException($"User {userId} was not attached to the organization Id:{Id}");
        }

        Users.Remove(orgUser);
        return this;
    }

    public JoinToOrgInvitation GetInvitationFor(
        User user) =>
        GetInvitationFor(user.Id);

    public JoinToOrgInvitation GetInvitationFor(
        long userId) =>
        Invitations.FirstOrDefault(x => x.InvitedUserId == userId);

    public bool HasUserBeenInvited(
        User user) =>
        HasUserBeenInvited(user.Id);

    public bool HasUserBeenInvited(
        long userId) =>
        Invitations != null &&
        Invitations.Any(x => x.InvitedUserId == userId &&
                             x.Status == InvitationStatus.Pending);

    public bool HasInvitationFor(
        User user) =>
        Invitations.Any(x => x.InvitedUserId == user.Id);
}