using System;

namespace MG.Utils.Abstract.NonNullableObjects
{
    public record NonNullableInt
    {
        private readonly string _source;
        private int? _value;

        /// <summary>
        /// Null is not allowed.
        /// </summary>
        /// <param name="source">Source.</param>
        public NonNullableInt(string source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public int ToInt()
        {
            if (_value == null)
            {
                if (int.TryParse(_source, out var result))
                {
                    _value = result;
                }
                else
                {
                    throw new InvalidCastException($"Could not cast '{_source}' as integer");
                }
            }

            return _value.Value;
        }

        public bool Equals(long second) => second == ToInt();

        public static implicit operator int(NonNullableInt nnInt) => nnInt.ToInt();
    }
}