using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MG.Utils.Helpers
{
    public class Constants : IReadOnlyCollection<FieldInfo>
    {
        private readonly IReadOnlyCollection<FieldInfo> _fields;

        public Constants(Type type)
        {
            _fields = type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .ToArray();
        }

        public IEnumerator<FieldInfo> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _fields.Count;

        public IReadOnlyCollection<string> Names()
        {
            return _fields.Select(x => x.Name).ToArray();
        }
    }
}