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

namespace Web.Api.Features.CompanyReviews.DeleteCompanyReview;

public class DeleteCompanyReviewHandler : IRequestHandler<DeleteCompanyReviewCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;
    private readonly ITechinterviewEmailService _emailService;
    private readonly ILogger<DeleteCompanyReviewHandler> _logger;

    public DeleteCompanyReviewHandler(
        DatabaseContext context,
        IAuthorization authorization,
        ITechinterviewEmailService emailService,
        ILogger<DeleteCompanyReviewHandler> logger)
    {
        _context = context;
        _authorization = authorization;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        DeleteCompanyReviewCommand request,
        CancellationToken cancellationToken)
    {
        await _authorization.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        var review = await _context.CompanyReviews
                         .Include(review => review.Company)
                         .Include(review => review.User)
                         .FirstOrDefaultAsync(
                             x =>
                                 x.Id == request.ReviewId &&
                                 x.CompanyId == request.CompanyId,
                             cancellationToken)
                     ?? throw new NotFoundException("Review not found");

        _context.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);

        if (review.User != null)
        {
            if (review.User.IsGoogleAuth() || review.User.IsGithubAuth())
            {
                await _emailService.CompanyReviewWasApprovedAsync(
                    review.User.Email,
                    review.Company.Name,
                    cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "Email about rejection was not sent to user {UserId} with email {Email} because they are not authenticated via Google or GitHub.",
                    review.UserId,
                    review.User.Email);
            }
        }
        else
        {
            _logger.LogWarning(
                "Review with ID {ReviewId} was rejected, but user with ID {UserId} does not exist or has no email.",
                review.Id,
                review.UserId);
        }

        return Unit.Value;
    }
}