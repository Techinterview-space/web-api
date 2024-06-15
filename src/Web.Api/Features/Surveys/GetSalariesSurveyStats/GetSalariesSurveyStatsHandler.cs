using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Surveys.Services;

namespace Web.Api.Features.Surveys.GetSalariesSurveyStats;

public class GetSalariesSurveyStatsHandler
    : IRequestHandler<GetSalariesSurveyStatsQuery, SalariesSurveyStatsData>
{
    private readonly DatabaseContext _context;

    public GetSalariesSurveyStatsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<SalariesSurveyStatsData> Handle(
        GetSalariesSurveyStatsQuery request,
        CancellationToken cancellationToken)
    {
        var createdAt = DateTime.UtcNow.AddDays(-SalariesSurveyUserService.RecentRepliesDays);
        var records = await _context.SalariesSurveyReplies
            .Where(x => x.CreatedAt >= createdAt)
            .AsNoTracking()
            .Select(x => new SalariesSurveyStatsDbData
            {
                UsefulnessReply = x.UsefulnessReply,
                ExpectationReply = x.ExpectationReply,
            })
            .ToListAsync(cancellationToken);

        var totalCount = records.Count;

        return new SalariesSurveyStatsData
        {
            CountOfRecords = records.Count,
            UsefulnessData = new List<SalariesSurveyStatsData.ReplyDataItem<SurveyUsefulnessReplyType>>
            {
                new (
                    SurveyUsefulnessReplyType.Yes,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.UsefulnessReply == SurveyUsefulnessReplyType.Yes),
                        totalCount)),
                new (
                    SurveyUsefulnessReplyType.No,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.UsefulnessReply == SurveyUsefulnessReplyType.No),
                        totalCount)),
                new (
                    SurveyUsefulnessReplyType.NotSure,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.UsefulnessReply == SurveyUsefulnessReplyType.NotSure),
                        totalCount)),
            },
            ExpectationData = new List<SalariesSurveyStatsData.ReplyDataItem<ExpectationReplyType>>
            {
                new (
                    ExpectationReplyType.Expected,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.ExpectationReply == ExpectationReplyType.Expected),
                        totalCount)),
                new (
                    ExpectationReplyType.MoreThanExpected,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.ExpectationReply == ExpectationReplyType.MoreThanExpected),
                        totalCount)),
                new (
                    ExpectationReplyType.LessThanExpected,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.ExpectationReply == ExpectationReplyType.LessThanExpected),
                        totalCount)),
            },
        };
    }
}