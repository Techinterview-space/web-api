using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData.Salary;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.SalarySubscribtions.ActivateSubscription;

public class ActivateSalarySubscriptionHandler : IRequestHandler<ActivateSalarySubscriptionCommand, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public ActivateSalarySubscriptionHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Nothing> Handle(
        ActivateSalarySubscriptionCommand request,
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
        return Nothing.Value;
    }
}