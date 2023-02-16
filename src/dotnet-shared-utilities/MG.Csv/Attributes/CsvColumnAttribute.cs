namespace MG.Csv.Attributes;

public class CsvColumnAttribute : Attribute
{
    public CsvColumnAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}