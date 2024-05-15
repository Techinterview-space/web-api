using System;
using Domain.Entities.Users;

namespace Domain.Entities.Questions;

public class SalariesSurveyReply : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public SurveyUsefulnessReplyType UsefulnessReply { get; protected set; }

    public ExpectationReplyType ExpectationReply { get; protected set; }

    public long? CreatedByUserId { get; protected set; }

    public virtual User CreatedByUser { get; protected set; }

    public SalariesSurveyReply(
        SurveyUsefulnessReplyType usefulnessReply,
        ExpectationReplyType expectationReply,
        User createdByUser)
    {
        UsefulnessReply = usefulnessReply;
        ExpectationReply = expectationReply;
        CreatedByUserId = createdByUser?.Id;
    }

    protected SalariesSurveyReply()
    {
    }
}