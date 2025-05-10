using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Dashboard.Dtos;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Dashboard;

[ApiController]
[Route("api/admin/dashboard")]
[HasAnyRole(Role.Admin)]
public class AdminDashboardController : ControllerBase
{
    private readonly DatabaseContext _context;

    public AdminDashboardController(
        DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<AdminDashboardData> GetDashboard(
        CancellationToken cancellationToken)
    {
        var yearAgo = DateTimeOffset.UtcNow.AddYears(-1);
        var feedbackReviews = await _context.SalariesSurveyReplies
            .Where(x => x.CreatedAt >= yearAgo)
            .Select(x => x.UsefulnessRating)
            .ToListAsync(cancellationToken);

        var salariesCount = await _context.Salaries
            .Where(x =>
                x.CreatedAt >= yearAgo &&
                x.UseInStats)
            .CountAsync(cancellationToken);

        var reviewsCount = await _context.CompanyReviews
            .Where(x =>
                x.CreatedAt >= yearAgo &&
                x.ApprovedAt != null &&
                x.OutdatedAt == null)
            .CountAsync(cancellationToken);

        return new AdminDashboardData(
            new AverageRatingData(feedbackReviews),
            salariesCount,
            reviewsCount);
    }
}