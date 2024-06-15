using System;
using System.Text;

namespace Web.Api.Features.Salaries.ExportCsv;

public record SalariesCsvResponse
{
    private readonly string _csvContent;

    public SalariesCsvResponse(
        string csvContent)
    {
        _csvContent = csvContent;
        Filename = $"salaries_{DateTime.UtcNow:yyyy-MM-dd_HH-mm}.csv";
        FileContentType = "text/csv";
    }

    public string CsvContent => _csvContent;

    public string Filename { get; }

    public string FileContentType { get; }

    public byte[] GetAsByteArray()
    {
        return Encoding.UTF8.GetBytes(_csvContent);
    }
}