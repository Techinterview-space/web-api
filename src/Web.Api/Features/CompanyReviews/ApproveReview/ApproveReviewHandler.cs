using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviews.ApproveReview;

public class ApproveReviewHandler : IRequestHandler<ApproveReviewCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public ApproveReviewHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Unit> Handle(
        ApproveReviewCommand request,
        CancellationToken cancellationToken)
    {
        await _authorization.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        var company = await _context.Companies
                          .Include(x => x.Reviews)
                          .Include(x => x.RatingHistory)
                          .Where(x => x.DeletedAt == null)
                          .FirstOrDefaultAsync(
                              x => x.Id == request.CompanyId,
                              cancellationToken)
                      ?? throw new NotFoundException("Company not found");

        company.ApproveReview(request.ReviewId);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}