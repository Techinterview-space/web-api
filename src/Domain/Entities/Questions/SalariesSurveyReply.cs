using System;
using System.Collections.Generic;
using Domain.Entities.Users;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Questions;

public class SalariesSurveyReply : HasDatesBase, IHasIdBase<Guid>
{
    public const int MinimumRating = 1;
    public const int MaximumRating = 5;

    public static readonly List<int> RatingValues = new ()
    {
        1, 2, 3, 4, 5,
    };

    public Guid Id { get; protected set; }

    public int UsefulnessRating { get; protected set; }

    public long? CreatedByUserId { get; protected set; }

    public virtual User CreatedByUser { get; protected set; }

    public SalariesSurveyReply(
        int usefulnessRating,
        User createdByUser)
    {
        if (usefulnessRating is < MinimumRating or > MaximumRating)
        {
            throw new BadRequestException(
                "Usefulness rating must be between 1 and 5.");
        }

        UsefulnessRating = usefulnessRating;
        CreatedByUserId = createdByUser?.Id;
    }

    protected SalariesSurveyReply()
    {
    }
}