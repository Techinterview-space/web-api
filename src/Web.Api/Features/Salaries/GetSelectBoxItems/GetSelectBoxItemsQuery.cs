using MediatR;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.GetSelectBoxItems;

public record GetSelectBoxItemsQuery : IRequest<SelectBoxItemsResponse>;