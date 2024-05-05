using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace TechInterviewer.Features.Surveys.Services;

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
        var createdAtEdge = DateTime.UtcNow.AddDays(-RecentRepliesDays);
        return _context.SalariesSurveyReplies
            .Where(x =>
                x.CreatedByUserId == currentUser.Id &&
                x.CreatedAt >= createdAtEdge)
            .AnyAsync(cancellationToken);
    }
}