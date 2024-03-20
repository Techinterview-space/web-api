using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Features.Labels.Models;
using TechInterviewer.Features.Salaries.Models;

namespace TechInterviewer.Features.Salaries.GetSelectBoxItems;

public class GetSelectBoxItemsHandler : IRequestHandler<GetSelectBoxItemsQuery, SelectBoxItemsResponse>
{
    private readonly DatabaseContext _context;

    public GetSelectBoxItemsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<SelectBoxItemsResponse> Handle(
        GetSelectBoxItemsQuery request,
        CancellationToken cancellationToken)
    {
        return new SelectBoxItemsResponse
        {
            Skills = await _context.Skills
                .Select(x => new LabelEntityDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    HexColor = x.HexColor,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken),
            Industries = await _context.WorkIndustries
                .Select(x => new LabelEntityDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    HexColor = x.HexColor,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken),
            Professions = await _context.Professions
                .Select(x => new LabelEntityDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    HexColor = x.HexColor,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken),
        };
    }
}