using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Emails.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.CompanyReviews.ApproveReview;

public class ApproveReviewHandler : IRequestHandler<ApproveReviewCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;
    private readonly ITechinterviewEmailService _emailService;
    private readonly ILogger<ApproveReviewHandler> _logger;

    public ApproveReviewHandler(
        DatabaseContext context,
        IAuthorization authorization,
        ITechinterviewEmailService emailService,
        ILogger<ApproveReviewHandler> logger)
    {
        _context = context;
        _authorization = authorization;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        ApproveReviewCommand request,
        CancellationToken cancellationToken)
    {
        await _authorization.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        var company = await _context.Companies
                          .Include(x => x.Reviews)
                          .ThenInclude(x => x.User)
                          .Include(x => x.RatingHistory)
                          .Where(x => x.DeletedAt == null)
                          .FirstOrDefaultAsync(
                              x => x.Id == request.CompanyId,
                              cancellationToken)
                      ?? throw new NotFoundException("Company not found");

        company.ApproveReview(request.ReviewId);
        await _context.SaveChangesAsync(cancellationToken);

        var review = company.GetReview(request.ReviewId);

        if (review.User != null)
        {
            if (review.User.IsGoogleAuth() || review.User.IsGithubAuth())
            {
                await _emailService.CompanyReviewWasApprovedAsync(
                    review.User.Email,
                    company.Name,
                    cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "Email about approval was not sent to user {UserId} with email {Email} because they are not authenticated via Google or GitHub.",
                    review.UserId,
                    review.User.Email);
            }
        }
        else
        {
            _logger.LogWarning(
                "Review with ID {ReviewId} was approved, but user with ID {UserId} does not exist or has no email.",
                review.Id,
                review.UserId);
        }

        return Unit.Value;
    }
}