namespace Web.Api.Features.Import.ImportKolesaCsv;

public record KolesaDeveloperCsvLine
{
    private readonly string _line;

    public KolesaDeveloperCsvLine(
        string line)
    {
        _line = line;
    }

    public override string ToString()
    {
        return _line;
    }
}