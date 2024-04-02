﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Features.Labels.Models;
using TechInterviewer.Features.Salaries.Models;
using TechInterviewer.Features.Salaries.Providers;

namespace Web.Api.Tests.Features.Salaries.ExportCsv;

public class SalaryLabelsProviderFake : ISalaryLabelsProvider
{
    private readonly DatabaseContext _context;

    public SalaryLabelsProviderFake(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<SelectBoxItemsResponse> GetAsync(
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