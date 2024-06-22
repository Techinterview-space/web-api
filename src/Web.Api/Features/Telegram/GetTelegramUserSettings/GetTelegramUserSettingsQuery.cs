using Domain.ValueObjects.Pagination;
using MediatR;

namespace Web.Api.Features.Telegram.GetTelegramUserSettings;

public record GetTelegramUserSettingsQuery : PageModel, IRequest<Pageable<TelegramUserSettingsDto>>;