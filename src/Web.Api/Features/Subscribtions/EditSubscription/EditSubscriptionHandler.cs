using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Subscribtions.GetStatDataChangeSubscriptions;

namespace Web.Api.Features.Subscribtions.EditSubscription;

public class EditSubscriptionHandler : Infrastructure.Services.Mediator.IRequestHandler<EditSubscriptionCommand, StatDataChangeSubscriptionDto>
{
    private readonly DatabaseContext _context;

    public EditSubscriptionHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<StatDataChangeSubscriptionDto> Handle(
        EditSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        if (string.IsNullOrEmpty(request.Name))
        {
            throw new BadRequestException("Name is required.");
        }

        var existingSubscription = await _context.StatDataChangeSubscriptions
            .FirstOrDefaultAsync(
                x => x.Id == request.SubscriptionId,
                cancellationToken);

        if (existingSubscription == null)
        {
            throw new NotFoundException("Subscription does not exist.");
        }

        var professions = new List<long>();
        if (request.ProfessionIds is { Count: > 0 })
        {
            professions = await _context.Professions
                .Where(x => request.ProfessionIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        existingSubscription.Update(
            request.Name,
            professions,
            request.PreventNotificationIfNoDifference,
            request.Regularity,
            request.UseAiAnalysis);

        await _context.SaveChangesAsync(cancellationToken);
        return new StatDataChangeSubscriptionDto(existingSubscription);
    }
}