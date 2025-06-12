using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Subscribtions.DeleteSubscription;

public class DeleteSubscriptionHandler
    : Infrastructure.Services.Mediator.IRequestHandler<DeleteSubscriptionCommand, Nothing>
{
    private readonly DatabaseContext _context;

    public DeleteSubscriptionHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Nothing> Handle(
        DeleteSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var cacheRecord = await _context.StatDataChangeSubscriptions
                              .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                          ?? throw NotFoundException.CreateFromEntity<StatDataChangeSubscription>(request.Id);

        _context.StatDataChangeSubscriptions.Remove(cacheRecord);

        await _context.SaveChangesAsync(cancellationToken);
        return Nothing.Value;
    }
}