using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Salaries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Import.ImportKolesaCsv;

public class ImportKolesaDevelopersCsvHandler
    : IRequestHandler<ImportKolesaDevelopersCsvCommand, List<ImportCsvResponseItem>>
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

        var salariesToSave = csv
            .GetRecords<KolesaDeveloperCsvLine>()
            .Where(x => x.UseInStat)
            .Select(x => new ImportCsvResponseItem
            {
                CsvLine = x,
                UserSalary =
                    new UserSalaryDto(
                        x.CreateUserSalary(
                            skills,
                            workIndustries,
                            professions)),
            })
            .ToList();

        return salariesToSave;
    }
}