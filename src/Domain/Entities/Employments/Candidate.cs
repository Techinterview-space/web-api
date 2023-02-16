using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Organizations;
using Domain.Entities.Users;

namespace Domain.Entities.Employments;

public class Candidate : HasDatesBase, IHasIdBase<Guid>
{
    protected Candidate()
    {
    }

    public Candidate(
        string firstName,
        string lastName,
        string contacts,
        Guid organizationId,
        User createdByOrNull = null)
    {
        Id = Guid.NewGuid();
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        Contacts = contacts?.Trim();
        CreatedById = createdByOrNull?.Id;
        OrganizationId = organizationId;
    }

    public Guid Id { get; protected set; }

    [Required]
    [StringLength(200)]
    public string FirstName { get; protected set; }

    [Required]
    [StringLength(200)]
    public string LastName { get; protected set; }

    [StringLength(5000)]
    public string Contacts { get; protected set; }

    // TODO Maxim: remove
    public IList<string> CvFiles { get; protected set; } = new List<string>();

    public long? CreatedById { get; protected set; }

    public virtual User CreatedBy { get; protected set; }

    public Guid OrganizationId { get; protected set; }

    public virtual Organization Organization { get; protected set; }

    public virtual ICollection<CandidateCard> CandidateCards { get; protected set; } = new List<CandidateCard>();

    public DateTimeOffset? DeletedAt { get; protected set; }

    [NotMapped]
    public string Fullname => FirstName + " " + LastName;

    [NotMapped]
    public bool Active => !DeletedAt.HasValue;

    public void Update(
        string firstName,
        string lastName,
        string contacts)
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        Contacts = contacts?.Trim();
    }

    public void Archive()
    {
        if (DeletedAt.HasValue)
        {
            throw new InvalidOperationException("Candidate is already deleted");
        }

        DeletedAt = DateTimeOffset.Now;
    }

    public void Restore()
    {
        if (Active)
        {
            throw new InvalidOperationException("Candidate is already activated");
        }

        DeletedAt = null;
    }
}