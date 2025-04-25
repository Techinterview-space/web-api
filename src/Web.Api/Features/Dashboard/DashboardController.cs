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
public class DashboardController : ControllerBase
{
    private readonly DatabaseContext _context;

    public DashboardController(
        DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<AdminDashboardData> GetDashboard(
        CancellationToken cancellationToken)
    {
        var feedbackReviews = await _context.SalariesSurveyReplies
            .Select(x => x.UsefulnessRating)
            .ToListAsync(cancellationToken);

        return new AdminDashboardData(
            new AverageRatingData(feedbackReviews));
    }
}