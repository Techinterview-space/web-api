using Domain.ValueObjects.Pagination;
using MediatR;

namespace Web.Api.Features.Telegram.GetTelegramBotUsages;

public record GetTelegramBotUsagesQuery : PageModel, IRequest<Pageable<TelegramBotUsageDto>>;