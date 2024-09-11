using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Telegram.ActivateCacheData;

namespace Web.Api.Features.Telegram.DeactivateCacheData;

public class DeactivateCacheDataHandler : IRequestHandler<ActivateCacheDataCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public DeactivateCacheDataHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Unit> Handle(
        ActivateCacheDataCommand request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _authorization.CurrentUserOrFailAsync(cancellationToken);
        if (!currentUser.Has(Role.Admin))
        {
            throw new NoPermissionsException("You don't have permissions to perform this.");
        }

        var cacheRecord = await _context.StatDataCacheRecords
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<StatDataCache>(request.Id);

        cacheRecord.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}