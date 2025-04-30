using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Companies.DeleteCompanyReview;

public class DeleteCompanyReviewHandler : IRequestHandler<DeleteCompanyReviewCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public DeleteCompanyReviewHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Unit> Handle(
        DeleteCompanyReviewCommand request,
        CancellationToken cancellationToken)
    {
        await _authorization.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        var review = await _context.CompanyReviews
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ReviewId &&
                    x.CompanyId == request.CompanyId,
                cancellationToken)
            ?? throw new NotFoundException("Review not found");

        _context.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}