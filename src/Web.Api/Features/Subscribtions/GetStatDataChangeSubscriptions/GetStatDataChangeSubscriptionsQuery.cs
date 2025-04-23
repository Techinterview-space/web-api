using Domain.ValueObjects.Pagination;
using MediatR;

namespace Web.Api.Features.Subscribtions.GetStatDataChangeSubscriptions;

public record GetStatDataChangeSubscriptionsQuery
    : PageModel, IRequest<Pageable<StatDataChangeSubscriptionDto>>;