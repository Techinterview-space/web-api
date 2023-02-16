using System;
using System.ComponentModel.DataAnnotations;
using Domain.Entities.Interviews;
using Domain.Entities.Users;
using MG.Utils.Abstract.Entities;
using MG.Utils.Entities;

namespace Domain.Entities.Employments;

public class CandidateInterview : HasDatesBase, IHasIdBase<Guid>
{
    protected CandidateInterview()
    {
    }

    public CandidateInterview(
        CandidateCard candidateCard,
        string comments,
        EmploymentStatus? conductedDuringStatus,
        Interview interview,
        User organizedBy)
    {
        CandidateCardId = candidateCard.Id;
        InterviewId = interview?.Id;
        OrganizedById = organizedBy?.Id;
        Comments = comments;
        ConductedDuringStatus = conductedDuringStatus;

        if (candidateCard.Id == default)
        {
            CandidateCard = candidateCard;
        }
    }

    public Guid Id { get; protected set; }

    public EmploymentStatus? ConductedDuringStatus { get; protected set; }

    [StringLength(3000)]
    public string Comments { get; protected set; }

    public Guid CandidateCardId { get; protected set; }

    public virtual CandidateCard CandidateCard { get; protected set; }

    public Guid? InterviewId { get; protected set; }

    public virtual Interview Interview { get; protected set; }

    public long? OrganizedById { get; protected set; }

    public virtual User OrganizedBy { get; protected set; }
}