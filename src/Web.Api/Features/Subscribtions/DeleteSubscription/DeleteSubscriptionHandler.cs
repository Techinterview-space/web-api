using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Subscribtions.DeleteSubscription;

public class DeleteSubscriptionHandler
    : IRequestHandler<DeleteSubscriptionCommand, Unit>
{
    private readonly DatabaseContext _context;

    public DeleteSubscriptionHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        DeleteSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var cacheRecord = await _context.StatDataChangeSubscriptions
                              .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                          ?? throw NotFoundException.CreateFromEntity<StatDataChangeSubscription>(request.Id);

        _context.StatDataChangeSubscriptions.Remove(cacheRecord);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}