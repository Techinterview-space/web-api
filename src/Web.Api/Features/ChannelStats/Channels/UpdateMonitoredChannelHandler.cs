using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.ChannelStats.Channels;

public class UpdateMonitoredChannelHandler
    : Infrastructure.Services.Mediator.IRequestHandler<UpdateMonitoredChannelCommand, MonitoredChannelDto>
{
    private readonly DatabaseContext _context;

    public UpdateMonitoredChannelHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<MonitoredChannelDto> Handle(
        UpdateMonitoredChannelCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ChannelName))
        {
            throw new BadRequestException("ChannelName is required.");
        }

        var entity = await _context.MonitoredChannels
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Monitored channel with ID {request.Id} not found.");
        }

        if (request.DiscussionChatExternalId.HasValue)
        {
            var duplicateDiscussionChat = await _context.MonitoredChannels
                .AnyAsync(
                    x => x.Id != request.Id
                         && x.DiscussionChatExternalId == request.DiscussionChatExternalId,
                    cancellationToken);

            if (duplicateDiscussionChat)
            {
                throw new BadRequestException(
                    $"Discussion chat with external ID {request.DiscussionChatExternalId.Value} is already mapped to another channel.");
            }
        }

        entity.Update(request.ChannelName, request.DiscussionChatExternalId);

        if (request.IsActive)
        {
            entity.Activate();
        }
        else
        {
            entity.Deactivate();
        }

        await _context.TrySaveChangesAsync(cancellationToken);

        return new MonitoredChannelDto(entity);
    }
}
