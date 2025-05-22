using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviews.VoteForReview;

public class VoteForReviewHandler
    : IRequestHandler<VoteForReviewCommand, VoteForReviewResponse>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public VoteForReviewHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<VoteForReviewResponse> Handle(
        VoteForReviewCommand request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();
        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var hasReviewAlready = await _context.CompanyReviewVotes
            .Where(x =>
                x.ReviewId == request.ReviewId &&
                x.UserId == user.Id)
            .AnyAsync(cancellationToken);

        if (hasReviewAlready)
        {
            return new VoteForReviewResponse(false);
        }

        var review = await _context.CompanyReviews
                         .Include(x => x.Votes)
                         .Where(x => x.Id == request.ReviewId)
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw NotFoundException.CreateFromEntity<CompanyReview>(request.ReviewId);

        review.AddVote(
            user,
            request.VoteType);

        await _context.SaveChangesAsync(cancellationToken);
        return new VoteForReviewResponse(true);
    }
}