namespace MG.Utils.Abstract.NonNullableObjects
{
    public record NonNullableString : NonNullable<string>
    {
        public NonNullableString(string value)
            : base(value)
        {
        }

        public NonNullableString(string value, string paramName)
            : base(value, paramName)
        {
        }

        public static implicit operator string(NonNullableString nnString)
        {
            return nnString.Value();
        }

        public override string ToString()
        {
            return Value();
        }
    }
}