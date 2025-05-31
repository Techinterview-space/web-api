using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Emails.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviews.ApproveReview;

public class ApproveReviewHandler : IRequestHandler<ApproveReviewCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;
    private readonly ITechinterviewEmailService _emailService;

    public ApproveReviewHandler(
        DatabaseContext context,
        IAuthorization authorization,
        ITechinterviewEmailService emailService)
    {
        _context = context;
        _authorization = authorization;
        _emailService = emailService;
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
        var review = company.GetReview(request.ReviewId);

        review.User?.GenerateNewEmailUnsubscribeTokenIfNecessary();

        await _context.SaveChangesAsync(cancellationToken);

        if (review.User != null &&
            await _emailService.CompanyReviewWasApprovedAsync(
                review.User,
                company.Name,
                cancellationToken))
        {
            await _context.SaveAsync(
                new UserEmail(
                    "Отзыв был одобрен",
                    UserEmailType.CompanyReviewNotification,
                    review.User),
                cancellationToken);
        }

        return Unit.Value;
    }
}