using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Telegram.ActivateStatDataChangeSubscription;

namespace Web.Api.Features.Telegram.DeactivateStatDataChangeSubscription;

public class DeactivateStatDataChangeSubscriptionHandler : IRequestHandler<DeactivateStatDataChangeSubscriptionCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public DeactivateStatDataChangeSubscriptionHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Unit> Handle(
        DeactivateStatDataChangeSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _authorization.CurrentUserOrFailAsync(cancellationToken);
        if (!currentUser.Has(Role.Admin))
        {
            throw new NoPermissionsException("You don't have permissions to perform this.");
        }

        var cacheRecord = await _context.StatDataChangeSubscriptions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<StatDataChangeSubscription>(request.Id);

        cacheRecord.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}