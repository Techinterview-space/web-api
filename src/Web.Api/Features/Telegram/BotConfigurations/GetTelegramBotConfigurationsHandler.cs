using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.BotConfigurations;

public class GetTelegramBotConfigurationsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetTelegramBotConfigurationsQuery, List<TelegramBotConfigurationDto>>
{
    private readonly DatabaseContext _context;

    public GetTelegramBotConfigurationsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<TelegramBotConfigurationDto>> Handle(
        GetTelegramBotConfigurationsQuery request,
        CancellationToken cancellationToken)
    {
        var configs = await _context.TelegramBotConfigurations
            .AsNoTracking()
            .OrderBy(x => x.BotType)
            .ToListAsync(cancellationToken);

        return configs
            .Select(x => new TelegramBotConfigurationDto(x))
            .ToList();
    }
}
