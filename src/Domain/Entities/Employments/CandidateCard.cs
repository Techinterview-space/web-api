using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Domain.Entities.Labels;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Domain.Services.Organizations;
using MG.Utils.Abstract.Entities;

namespace Domain.Entities.Employments;

public class CandidateCard : HasLabelsEntity<CandidateCard, OrganizationLabel>, IHasIdBase<Guid>
{
    protected CandidateCard()
    {
    }

    public CandidateCard(
        Candidate candidate,
        User openBy = null,
        EmploymentStatus employmentStatus = EmploymentStatus.LongList)
    {
        Id = Guid.NewGuid();
        CandidateId = candidate.Id;
        OrganizationId = candidate.OrganizationId;
        OpenById = openBy?.Id;
        EmploymentStatus = employmentStatus;
    }

    public Guid Id { get; protected set; }

    public EmploymentStatus EmploymentStatus { get; protected set; }

    public Guid CandidateId { get; protected set; }

    public virtual Candidate Candidate { get; protected set; }

    public long? OpenById { get; protected set; }

    public virtual User OpenBy { get; protected set; }

    public Guid OrganizationId { get; protected set; }

    public virtual Organization Organization { get; protected set; }

    public virtual ICollection<CandidateInterview> Interviews { get; protected set; } = new List<CandidateInterview>();

    public virtual ICollection<CandidateCardComment> Comments { get; protected set; } = new List<CandidateCardComment>();

    public List<CandidateCardCvFile> Files { get; protected set; } = new ();

    public DateTimeOffset? DeletedAt { get; protected set; }

    [NotMapped]
    public bool Active => !DeletedAt.HasValue;

    public void Archive()
    {
        if (DeletedAt.HasValue)
        {
            throw new InvalidOperationException($"CandidateCard Id:{Id} is already deleted");
        }

        DeletedAt = DateTimeOffset.Now;
    }

    public void Restore()
    {
        if (!DeletedAt.HasValue)
        {
            throw new InvalidOperationException($"CandidateCard Id:{Id} is active");
        }

        DeletedAt = null;
    }

    public CandidateCard Update(
        EmploymentStatus status)
    {
        EmploymentStatus = status;
        return this;
    }

    public void AddFile(
        string fileName,
        string storageFileName)
    {
        Files ??= new List<CandidateCardCvFile>();
        Files.Add(new CandidateCardCvFile(fileName, storageFileName));
    }

    public void RemoveFile(
        string storageFileName)
    {
        Files?.RemoveAll(f => f.StorageFileName == storageFileName);
    }

    public CandidateCardCvFile FindFile(
        Guid fileId)
    {
        return Files?.FirstOrDefault(x => x.Id == fileId);
    }
}