using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Domain.ValueObjects;

public record IntArrayParameter : IReadOnlyCollection<int>
{
    private readonly IReadOnlyCollection<int> _parsed;

    public IntArrayParameter(string source)
    {
        _parsed = source?.Split(',')
                      .Select(TryParseOrNull)
                      .Where(x => x.HasValue)
                      .Select(x => x.Value)
                      .ToArray()
                  ?? Array.Empty<int>();
    }

    public IEnumerator<int> GetEnumerator()
    {
        return _parsed.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _parsed.Count;

    private static int? TryParseOrNull(string value)
    {
        return int.TryParse(value, out int result)
            ? result
            : default(int?);
    }
}