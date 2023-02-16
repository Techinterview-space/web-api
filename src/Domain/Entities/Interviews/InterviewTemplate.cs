﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Labels;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Domain.Enums;
using MG.Utils.Abstract.Entities;

namespace Domain.Entities.Interviews;

public class InterviewTemplate : HasLabelsEntity<InterviewTemplate, UserLabel>, IHasIdBase<Guid>
{
    protected InterviewTemplate()
    {
    }

    public InterviewTemplate(
        string title,
        string overallOpinion,
        bool isPublic,
        List<InterviewTemplateSubject> subjects,
        User author,
        Guid? organizationId,
        ICollection<UserLabel> labels = null)
    {
        Title = title;
        OverallOpinion = overallOpinion;
        Subjects = subjects;
        AuthorId = author.Id;
        IsPublic = isPublic;
        OrganizationId = organizationId;
        Labels = labels ?? new List<UserLabel>();
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; protected set; }

    [Required]
    [StringLength(150)]
    public string Title { get; protected set; }

    [StringLength(Interview.OverallStringLength)]
    public string OverallOpinion { get; protected set; }

    public long AuthorId { get; protected set; }

    public virtual User Author { get; protected set; }

    public bool IsPublic { get; protected set; }

    public Guid? OrganizationId { get; protected set; }

    public virtual Organization Organization { get; protected set; }

    public List<InterviewTemplateSubject> Subjects { get; protected set; } = new ();

    public bool CouldBeEditBy(User user)
    {
        return AuthorId == user.Id ||
               user.Has(Role.Admin) ||
               (OrganizationId.HasValue && user.IsMyOrganization(OrganizationId.Value));
    }

    public bool CouldBeOpenBy(User user)
    {
        return AuthorId == user.Id ||
               user.Has(Role.Admin) ||
               IsPublic ||
               (OrganizationId.HasValue && user.IsMyOrganization(OrganizationId.Value));
    }

    public InterviewTemplate Update(
        string title,
        string overallOpinion,
        bool isPublic,
        List<InterviewTemplateSubject> subjects,
        Guid? organizationId)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        OverallOpinion = overallOpinion;
        Subjects = subjects;
        IsPublic = isPublic;
        OrganizationId = organizationId;

        return this;
    }
}