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
    : IRequestHandler<ImportKolesaDevelopersCsvCommand, List<UserSalaryDto>>
{
    private readonly DatabaseContext _context;

    public ImportKolesaDevelopersCsvHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public Task<List<UserSalaryDto>> Handle(
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

        var skills = _context.Skills
            .AsNoTracking()
            .ToList();

        var workIndustries = _context.WorkIndustries
            .AsNoTracking()
            .ToList();

        var professions = _context.Professions
            .AsNoTracking()
            .ToList();

        using var stream = request.File.OpenReadStream();
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(streamReader, config);

        var salariesToSave = csv
            .GetRecords<KolesaDeveloperCsvLine>()
            .Where(x => x.UseInStat)
            .Select(x => x.CreateUserSalary(
                skills,
                workIndustries,
                professions))
            .ToList();

        return Task.FromResult(salariesToSave
            .Select(x => new UserSalaryDto(x))
            .ToList());
    }
}