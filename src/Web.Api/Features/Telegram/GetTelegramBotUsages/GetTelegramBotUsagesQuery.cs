using Domain.ValueObjects.Pagination;
using MediatR;

namespace TechInterviewer.Features.Telegram.GetTelegramBotUsages;

public record GetTelegramBotUsagesQuery : PageModel, IRequest<Pageable<TelegramBotUsageDto>>;