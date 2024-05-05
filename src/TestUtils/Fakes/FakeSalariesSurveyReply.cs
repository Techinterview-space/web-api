using System;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Domain.Entities.Users;
using Infrastructure.Database;

namespace TestUtils.Fakes;

public class FakeSalariesSurveyReply : SalariesSurveyReply
{
    public FakeSalariesSurveyReply(
        SurveyUsefulnessReplyType usefulnessReply,
        ExpectationReplyType expectationReply,
        User user,
        DateTime? createdAt)
        : base(
            usefulnessReply,
            expectationReply,
            user)
    {
        CreatedAt = createdAt ?? DateTimeOffset.Now;
    }

    public async Task<SalariesSurveyReply> PleaseAsync(
        DatabaseContext context)
    {
        var entry = await context.SalariesSurveyReplies.AddAsync((SalariesSurveyReply)this);
        await context.TrySaveChangesAsync();
        return entry.Entity;
    }
}