using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Enums;
using Domain.Entities.Labels;
using Domain.Entities.Users;

namespace Domain.Entities.Interviews;

public class Interview : HasLabelsEntity<Interview, UserLabel>, IHasIdBase<Guid>
{
    public const int OverallStringLength = 50_000;

    protected Interview()
    {
    }

    public Interview(
        string candidateName,
        string overallOpinion,
        DeveloperGrade? grade,
        List<InterviewSubject> subjects,
        User interviewer)
    {
        CandidateName = candidateName;
        OverallOpinion = overallOpinion;
        CandidateGrade = grade;
        InterviewerId = interviewer.Id;
        Subjects = subjects;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; protected set; }

    [Required]
    [StringLength(200)]
    public string CandidateName { get; protected set; }

    public long InterviewerId { get; protected set; }

    public virtual User Interviewer { get; protected set; }

    [StringLength(OverallStringLength)]
    public string OverallOpinion { get; protected set; }

    public DeveloperGrade? CandidateGrade { get; protected set; }

    public virtual ShareLink ShareLink { get; protected set; }

    public List<InterviewSubject> Subjects { get; protected set; } = new ();

    public bool CouldBeOpenBy(User user)
        => InterviewerId == user.Id;

    public Interview Update(
        string candidateName,
        string overallOpinion,
        DeveloperGrade? grade,
        List<InterviewSubject> subjects)
    {
        CandidateName = candidateName;
        OverallOpinion = overallOpinion;
        CandidateGrade = grade;
        Subjects = subjects;

        return this;
    }
}