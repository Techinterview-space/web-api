using System;
using Domain.Entities.Users;

namespace Domain.Entities.Questions;

public class SalariesSurveyReply : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public SalariesSurveyReplyType ReplyType { get; protected set; }

    public Guid SalariesSurveyQuestionId { get; protected set; }

    public virtual SalariesSurveyQuestion SalariesSurveyQuestion { get; protected set; }

    public long? CreatedByUserId { get; protected set; }

    public virtual User CreatedByUser { get; protected set; }

    public SalariesSurveyReply(
        SalariesSurveyReplyType replyType,
        SalariesSurveyQuestion question,
        User createdByUser)
    {
        ReplyType = replyType;
        SalariesSurveyQuestionId = question.Id;
        CreatedByUserId = createdByUser?.Id;
    }

    protected SalariesSurveyReply()
    {
    }
}