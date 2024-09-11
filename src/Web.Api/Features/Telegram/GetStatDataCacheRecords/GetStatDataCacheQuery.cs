using Domain.ValueObjects.Pagination;
using MediatR;

namespace Web.Api.Features.Telegram.GetStatDataCacheRecords;

public record GetStatDataCacheQuery : PageModel, IRequest<Pageable<StatDataCacheDto>>;