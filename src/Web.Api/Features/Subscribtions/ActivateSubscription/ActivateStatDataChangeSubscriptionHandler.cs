using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Subscribtions.ActivateSubscription;

public class ActivateStatDataChangeSubscriptionHandler : IRequestHandler<ActivateStatDataChangeSubscriptionCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public ActivateStatDataChangeSubscriptionHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Unit> Handle(
        ActivateStatDataChangeSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);
        if (!currentUser.Has(Role.Admin))
        {
            throw new NoPermissionsException("You don't have permissions to perform this.");
        }

        var cacheRecord = await _context.StatDataChangeSubscriptions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<StatDataChangeSubscription>(request.Id);

        cacheRecord.Activate();

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}