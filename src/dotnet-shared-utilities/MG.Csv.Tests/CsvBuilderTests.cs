using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MG.Csv.Attributes;
using Xunit;

namespace MG.Csv.Tests;

public class CsvBuilderTests
{
    [Fact]
    public void AsString_ValidData_Ok()
    {
        var csv = new CsvBuilder<AwesomeClass>(new List<AwesomeClass>
        {
            new ("Maxim Gorbatyuk", 142.56),
            new ("John Smith", 5369),
        });

        var result = csv.AsString();
        Assert.Equal("Fullname,Money\nMaxim Gorbatyuk,142.56\nJohn Smith,5369\n", result);
    }

    [Fact]
    public void AsString_WithCsvColumns_Ok()
    {
        var csv = new CsvBuilder<AwesomeClassWithCsvColumns>(new List<AwesomeClassWithCsvColumns>
        {
            new ("Maxim Gorbatyuk", 142.56),
            new ("John Smith", 5369),
        });

        var result = csv.AsString();
        Assert.Equal("full_name,money\nMaxim Gorbatyuk,142.56\nJohn Smith,5369\n", result);
    }

    [Fact]
    public void AsBytes_WithCsvColumns_Ok()
    {
        var csv = new CsvBuilder<AwesomeClassWithCsvColumns>(new List<AwesomeClassWithCsvColumns>
        {
            new ("Maxim Gorbatyuk", 142.56),
            new ("John Smith", 5369),
        });

        var result = Encoding.UTF8.GetString(csv.AsBytes());
        Assert.Equal("full_name,money\nMaxim Gorbatyuk,142.56\nJohn Smith,5369\n", result);
    }

    [Fact]
    public async Task AsMemoryStream_WithCsvColumns_OkAsync()
    {
        var csv = new CsvBuilder<AwesomeClassWithCsvColumns>(new List<AwesomeClassWithCsvColumns>
        {
            new ("Maxim Gorbatyuk", 142.56),
            new ("John Smith", 5369),
        });

        await using var stream = csv.AsMemoryStream();
        using var reader = new StreamReader(stream);

        var result = await reader.ReadToEndAsync();
        Assert.Equal("full_name,money\nMaxim Gorbatyuk,142.56\nJohn Smith,5369\n", result);
    }

    private class AwesomeClass
    {
        public AwesomeClass()
        {
        }

        public AwesomeClass(
            string? fullname,
            double money)
        {
            Fullname = fullname;
            Money = money;
        }

        public virtual string? Fullname { get; init; }

        public virtual double Money { get; init; }
    }

    private class AwesomeClassWithCsvColumns : AwesomeClass
    {
        public AwesomeClassWithCsvColumns(
            string? fullname,
            double money)
        {
            Fullname = fullname;
            Money = money;
        }

        [CsvColumn("full_name")]
        public override string? Fullname { get; init; }

        [CsvColumn("money")]
        public override double Money { get; init; }
    }
}