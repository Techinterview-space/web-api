using System;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Domain.Entities.Users;
using Infrastructure.Database;

namespace TestUtils.Fakes;

public class FakeSalariesSurveyReply : SalariesSurveyReply
{
    public FakeSalariesSurveyReply(
        SalariesSurveyReplyType replyType,
        SalariesSurveyQuestion question,
        User user,
        DateTime? createdAt)
        : base(replyType, question, user)
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