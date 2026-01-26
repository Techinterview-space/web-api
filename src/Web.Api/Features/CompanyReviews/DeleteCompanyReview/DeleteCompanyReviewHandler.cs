using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Emails.Contracts;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviews.DeleteCompanyReview;

public class DeleteCompanyReviewHandler : IRequestHandler<DeleteCompanyReviewCommand, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;
    private readonly ITechinterviewEmailService _emailService;

    public DeleteCompanyReviewHandler(
        DatabaseContext context,
        IAuthorization authorization,
        ITechinterviewEmailService emailService)
    {
        _context = context;
        _authorization = authorization;
        _emailService = emailService;
    }

    public async Task<Nothing> Handle(
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
        review.User?.GenerateNewEmailUnsubscribeTokenIfNecessary();

        await _context.SaveChangesAsync(cancellationToken);

        if (review.User != null &&
            await _emailService.CompanyReviewWasRejectedAsync(
                review.User,
                review.Company.Name,
                cancellationToken))
        {
            await _context.SaveAsync(
                new UserEmail(
                    "Отзыв был отклонен",
                    UserEmailType.CompanyReviewNotification,
                    review.User),
                cancellationToken);
        }

        return Nothing.Value;
    }
}