using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Domain.Entities.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Surveys.Services;

public class SalariesSurveyUserService
{
    public const int RecentRepliesDays = 180;

    private readonly DatabaseContext _context;

    public SalariesSurveyUserService(
        DatabaseContext context)
    {
        _context = context;
    }

    public Task<bool> HasFilledSurveyAsync(
        User currentUser,
        CancellationToken cancellationToken = default)
    {
        return _context.SalariesSurveyReplies
            .Where(GetSurveyReplyFilter(currentUser))
            .AnyAsync(cancellationToken);
    }

    public Task<SalariesSurveyReply> GetLastSurveyOrNullAsync(
        User currentUser,
        CancellationToken cancellationToken = default)
    {
        return _context.SalariesSurveyReplies
            .Where(GetSurveyReplyFilter(currentUser))
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private Expression<Func<SalariesSurveyReply, bool>> GetSurveyReplyFilter(
        User currentUser)
    {
        var createdAtEdge = DateTime.UtcNow.AddDays(-RecentRepliesDays);
        return x =>
            x.CreatedByUserId == currentUser.Id &&
            x.CreatedAt >= createdAtEdge;
    }
}