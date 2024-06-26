using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using MediatR;

namespace Web.Api.Features.Import.ImportKolesaCsv;

public class ImportKolesaDevelopersCsvHandler
    : IRequestHandler<ImportKolesaDevelopersCsvCommand, List<KolesaDeveloperCsvLine>>
{
    public Task<List<KolesaDeveloperCsvLine>> Handle(
        ImportKolesaDevelopersCsvCommand request,
        CancellationToken cancellationToken)
    {
        var filename = request.File.FileName;
        var extension = Path.GetExtension(filename);

        if (extension != ".csv")
        {
            throw new BadRequestException("Invalid file extension");
        }

        var lines = ReadLines(request)
            .Select(x => new KolesaDeveloperCsvLine(x))
            .ToList();

        return Task.FromResult(lines);
    }

    private IEnumerable<string> ReadLines(
        ImportKolesaDevelopersCsvRequestBody request)
    {
        using var stream = request.File.OpenReadStream();
        using var streamReader = new StreamReader(stream, Encoding.UTF8);

        string line;
        while ((line = streamReader.ReadLine()) != null)
        {
            yield return line;
        }
    }
}