using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData.Salary;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviewsSubscriptions.DeactivateSubscription;

public class DeactivateCompanyReviewsSubscriptionHandler
    : IRequestHandler<DeactivateCompanyReviewsSubscriptionCommand, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public DeactivateCompanyReviewsSubscriptionHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Nothing> Handle(
        DeactivateCompanyReviewsSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);
        if (!currentUser.Has(Role.Admin))
        {
            throw new NoPermissionsException("You don't have permissions to perform this.");
        }

        var cacheRecord = await _context.SalariesSubscriptions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<StatDataChangeSubscription>(request.Id);

        cacheRecord.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
        return Nothing.Value;
    }
}