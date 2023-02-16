using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Employments;
using Domain.Entities.Enums;
using Domain.Entities.Labels;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using MG.Utils.Abstract.Entities;

namespace Domain.Entities.Interviews;

public class Interview : HasLabelsEntity<Interview, UserLabel>, IHasIdBase<Guid>
{
    public const int OverallStringLength = 20000;

    protected Interview()
    {
    }

    public Interview(
        string candidateName,
        string overallOpinion,
        DeveloperGrade? grade,
        List<InterviewSubject> subjects,
        User interviewer,
        Guid? organizationId)
    {
        CandidateName = candidateName;
        OverallOpinion = overallOpinion;
        CandidateGrade = grade;
        InterviewerId = interviewer.Id;
        Subjects = subjects;
        OrganizationId = organizationId;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; protected set; }

    [Required]
    [StringLength(150)]
    public string CandidateName { get; protected set; }

    public long InterviewerId { get; protected set; }

    public virtual User Interviewer { get; protected set; }

    [StringLength(OverallStringLength)]
    public string OverallOpinion { get; protected set; }

    public DeveloperGrade? CandidateGrade { get; protected set; }

    public List<InterviewSubject> Subjects { get; protected set; } = new ();

    public Guid? OrganizationId { get; protected set; }

    public virtual Organization Organization { get; protected set; }

    public virtual CandidateInterview CandidateInterview { get; protected set; }

    public bool CouldBeOpenBy(User user)
        => InterviewerId == user.Id ||
           (OrganizationId.HasValue && user.IsMyOrganization(OrganizationId.Value));

    public Interview Update(
        string candidateName,
        string overallOpinion,
        DeveloperGrade? grade,
        List<InterviewSubject> subjects,
        Guid? organizationId)
    {
        CandidateName = candidateName;
        OverallOpinion = overallOpinion;
        CandidateGrade = grade;
        Subjects = subjects;
        OrganizationId = organizationId;

        return this;
    }

    public Interview AddCardInterview(
        CandidateCard card,
        User currentUser)
    {
        CandidateInterview = new CandidateInterview(
            card,
            null,
            card.EmploymentStatus,
            this,
            currentUser);

        return this;
    }
}