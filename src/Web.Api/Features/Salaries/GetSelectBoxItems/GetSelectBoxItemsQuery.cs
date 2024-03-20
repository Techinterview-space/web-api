using MediatR;
using TechInterviewer.Features.Salaries.Models;

namespace TechInterviewer.Features.Salaries.GetSelectBoxItems;

public record GetSelectBoxItemsQuery : IRequest<SelectBoxItemsResponse>;