using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Features.Surveys.Services;

namespace TechInterviewer.Features.Surveys.GetSalariesSurveyStats;

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
            UsefulnessData = new Dictionary<SurveyUsefulnessReplyType, SalariesSurveyStatsDataItem>
            {
                {
                    SurveyUsefulnessReplyType.Yes,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.UsefulnessReply == SurveyUsefulnessReplyType.Yes),
                        totalCount)
                },
                {
                    SurveyUsefulnessReplyType.No,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.UsefulnessReply == SurveyUsefulnessReplyType.No),
                        totalCount)
                },
                {
                    SurveyUsefulnessReplyType.NotSure,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.UsefulnessReply == SurveyUsefulnessReplyType.NotSure),
                        totalCount)
                },
            },
            ExpectationData = new Dictionary<ExpectationReplyType, SalariesSurveyStatsDataItem>
            {
                {
                    ExpectationReplyType.Expected,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.ExpectationReply == ExpectationReplyType.Expected),
                        totalCount)
                },
                {
                    ExpectationReplyType.MoreThanExpected,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.ExpectationReply == ExpectationReplyType.MoreThanExpected),
                        totalCount)
                },
                {
                    ExpectationReplyType.LessThanExpected,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.ExpectationReply == ExpectationReplyType.LessThanExpected),
                        totalCount)
                },
            },
        };
    }
}