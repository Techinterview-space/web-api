using System.Reflection;
using System.Text;
using MG.Csv.Attributes;

namespace MG.Csv;

public class CsvBuilder<T>
{
    protected virtual Encoding Encoding { get; } = Encoding.UTF8;

    private readonly IEnumerable<T> _collection;
    private readonly IReadOnlyCollection<PropertyInfo> _properties;
    private readonly bool _addHeader;

    public CsvBuilder(IEnumerable<T> collection, bool addHeader = true)
    {
        _collection = collection;
        _addHeader = addHeader;
        _properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    public string AsString()
    {
        var stringBuilder = new StringBuilder();

        if (_addHeader)
        {
            stringBuilder.AppendLine(string.Join(",", _properties.Select(x => x.GetCustomAttribute<CsvColumnAttribute>()?.Name ?? x.Name)));
        }

        foreach (var record in _collection)
        {
            var values = _properties.Select(p => p.GetValue(record, null));
            stringBuilder.AppendLine(string.Join(",", values));
        }

        return stringBuilder.ToString();
    }

    public byte[] AsBytes() =>
        Encoding.GetBytes(AsString());

    public MemoryStream AsMemoryStream() =>
        new MemoryStream(AsBytes());
}