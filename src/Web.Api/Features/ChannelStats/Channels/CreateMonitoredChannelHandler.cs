using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.ChannelStats.Channels;

public class CreateMonitoredChannelHandler
    : Infrastructure.Services.Mediator.IRequestHandler<CreateMonitoredChannelRequest, MonitoredChannelDto>
{
    private readonly DatabaseContext _context;

    public CreateMonitoredChannelHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<MonitoredChannelDto> Handle(
        CreateMonitoredChannelRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ChannelName))
        {
            throw new BadRequestException("ChannelName is required.");
        }

        if (request.ChannelExternalId == 0)
        {
            throw new BadRequestException("ChannelExternalId is required.");
        }

        var duplicate = await _context.MonitoredChannels
            .AnyAsync(
                x => x.ChannelExternalId == request.ChannelExternalId,
                cancellationToken);

        if (duplicate)
        {
            throw new BadRequestException(
                $"Channel with external ID {request.ChannelExternalId} already exists.");
        }

        if (request.DiscussionChatExternalId.HasValue)
        {
            var duplicateDiscussionChat = await _context.MonitoredChannels
                .AnyAsync(
                    x => x.DiscussionChatExternalId == request.DiscussionChatExternalId,
                    cancellationToken);

            if (duplicateDiscussionChat)
            {
                throw new BadRequestException(
                    $"Discussion chat with external ID {request.DiscussionChatExternalId.Value} is already mapped to another channel.");
            }
        }

        var entity = new MonitoredChannel(
            request.ChannelExternalId,
            request.ChannelName,
            request.DiscussionChatExternalId);

        await _context.SaveAsync(entity, cancellationToken);

        return new MonitoredChannelDto(entity);
    }
}
