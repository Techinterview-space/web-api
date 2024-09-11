using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.ActivateCacheData;

public class ActivateCacheDataHandler : IRequestHandler<ActivateCacheDataCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public ActivateCacheDataHandler(
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

        cacheRecord.Activate();

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}