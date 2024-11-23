using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.GetTelegramBotUsages;

public class GetOrCreateTelegramBotUsageHandler : IRequestHandler<GetOrCreateTelegramBotUsageCommand, Unit>
{
    private readonly DatabaseContext _context;

    public GetOrCreateTelegramBotUsageHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(GetOrCreateTelegramBotUsageCommand request, CancellationToken cancellationToken)
    {
        var usage = await _context
        .TelegramBotUsages
        .FirstOrDefaultAsync(
                x => x.Username == request.UserName && x.UsageType == request.UsageType,
                cancellationToken);

        if (usage == null)
        {
            usage = new TelegramBotUsage(request.UserName, request.ChannelName, request.UsageType);
            _context.TelegramBotUsages.Add(usage);
        }

        usage.IncrementUsageCount(request.ReceivedMessageTextOrNull);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
