namespace MG.Utils.Abstract.NonNullableObjects
{
    public record Long
    {
        private readonly string _source;

        private long? _value;

        /// <summary>
        /// Null is not allowed.
        /// </summary>
        /// <param name="source">Source.</param>
        public Long(string source)
        {
            _source = source;
        }

        public long Value()
        {
            if (_value == null)
            {
                if (_source != null && long.TryParse(_source, out var result))
                {
                    _value = result;
                }
                else
                {
                    _value = default(long);
                }
            }

            return _value.Value;
        }

        public bool Equals(long second) => second == Value();
    }
}