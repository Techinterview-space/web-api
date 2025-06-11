using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        var createdAt = DateTime.UtcNow.AddYears(-1);
        var records = await _context.SalariesSurveyReplies
            .Where(x =>
                x.CreatedAt >= createdAt)
            .AsNoTracking()
            .Select(x => new SalariesSurveyStatsDbData
            {
                UsefulnessRating = x.UsefulnessRating,
            })
            .ToListAsync(cancellationToken);

        var totalCount = records.Count;

        return new SalariesSurveyStatsData
        {
            CountOfRecords = records.Count,
            UsefulnessData = SalariesSurveyReply.RatingValues
                .Select(ratingValue => new SalariesSurveyStatsData.ReplyDataItem(
                    ratingValue,
                    new SalariesSurveyStatsDataItem(
                        records.Count(x => x.UsefulnessRating == ratingValue),
                        totalCount)))
                .ToList(),
        };
    }
}