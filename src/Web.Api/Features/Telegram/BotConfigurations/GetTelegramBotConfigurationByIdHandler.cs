using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Telegram.BotConfigurations;

public class GetTelegramBotConfigurationByIdHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetTelegramBotConfigurationByIdQuery, TelegramBotConfigurationDto>
{
    private readonly DatabaseContext _context;

    public GetTelegramBotConfigurationByIdHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<TelegramBotConfigurationDto> Handle(
        GetTelegramBotConfigurationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.TelegramBotConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<TelegramBotConfiguration>(request.Id);

        return new TelegramBotConfigurationDto(entity);
    }
}
