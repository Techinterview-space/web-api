namespace MG.Utils.Abstract.Random
{
    public abstract record RandomStringBase
    {
        protected abstract string Chars { get; }

        private readonly int _length;
        private string _result;

        protected RandomStringBase(int length)
        {
            _length = length;
        }

        public string Get => _result ??= GetInternal();

        private string GetInternal()
        {
            var stringChars = new char[_length];
            var random = new System.Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = Chars[random.Next(Chars.Length)];
            }

            return new string(stringChars);
        }

        public static explicit operator string(RandomStringBase pass)
        {
            return pass?.Get;
        }
    }
}