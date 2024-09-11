using Domain.ValueObjects.Pagination;
using MediatR;

namespace Web.Api.Features.Telegram.GetStatDataChangeSubscriptions;

public record GetStatDataChangeSubscriptionsQuery : PageModel, IRequest<Pageable<StatDataChangeSubscriptionDto>>;