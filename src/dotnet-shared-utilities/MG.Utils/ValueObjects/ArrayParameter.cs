using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MG.Utils.ValueObjects
{
    public class ArrayParameter : IReadOnlyCollection<long>
    {
        private readonly IReadOnlyCollection<long> _parsed;

        public ArrayParameter(string source)
        {
            _parsed = source?.Split(',')
                            .Select(TryParseOrNull)
                            .Where(x => x.HasValue)
                            .Select(x => x.Value)
                            .ToArray()
                        ?? Array.Empty<long>();
        }

        public IEnumerator<long> GetEnumerator()
        {
            return _parsed.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _parsed.Count;

        private static long? TryParseOrNull(string value)
        {
            return long.TryParse(value, out long result)
                ? result
                : default(long?);
        }
    }
}