﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Entities.Salaries;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Import.ImportKolesaCsv;

public class ImportKolesaDevelopersCsvHandler
    : Infrastructure.Services.Mediator.IRequestHandler<ImportKolesaDevelopersCsvCommand, List<ImportCsvResponseItem>>
{
    private readonly DatabaseContext _context;

    public ImportKolesaDevelopersCsvHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<ImportCsvResponseItem>> Handle(
        ImportKolesaDevelopersCsvCommand request,
        CancellationToken cancellationToken)
    {
        var filename = request.File.FileName;
        var extension = Path.GetExtension(filename);

        if (extension != ".csv")
        {
            throw new BadRequestException("Invalid file extension");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            NewLine = Environment.NewLine,
            Encoding = Encoding.UTF8,
            Delimiter = ";",
        };

        var skills = await _context.Skills
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var workIndustries = await _context.WorkIndustries
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var professions = await _context.Professions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        await using var stream = request.File.OpenReadStream();
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(streamReader, config);

        var result = new List<(KolesaDeveloperCsvLine record, UserSalary salary)>();
        var records = csv.GetRecords<KolesaDeveloperCsvLine>();
        foreach (var record in records)
        {
            if (record.Salary == null ||
                record.GetCompanyTypeAsEnum() == null)
            {
                continue;
            }

            var salary = record.CreateUserSalary(
                skills,
                workIndustries,
                professions);

            if (salary != null)
            {
                _context.Salaries.Add(salary);
            }

            result.Add((record, salary));
        }

        await _context.TrySaveChangesAsync(cancellationToken);
        return result
            .Select(x => new ImportCsvResponseItem
            {
                CsvLine = x.record,
                UserSalary = x.salary != null ? new UserSalaryDto(x.salary) : null,
            })
            .ToList();
    }
}