using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviews.MarkReviewOutdated;

public class MarkReviewOutdatedHandler : IRequestHandler<MarkReviewOutdatedCommand, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public MarkReviewOutdatedHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Nothing> Handle(
        MarkReviewOutdatedCommand request,
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

        company.MarkReviewAsOutdated(request.ReviewId);
        await _context.SaveChangesAsync(cancellationToken);

        return Nothing.Value;
    }
}