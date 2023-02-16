using System.Text.RegularExpressions;
using MG.Utils.Exceptions;

namespace MG.Utils.Security
{
    public readonly struct ComplexityScore
    {
        public const int MinLength = 8;
        private const int MinSuccess = 3;

        private static readonly Regex _numeric = new (@"[\d]", RegexOptions.Compiled);
        private static readonly Regex _hasLetter = new (@"([a-z])", RegexOptions.Compiled);
        private static readonly Regex _hasCapital = new (@"([A-Z])", RegexOptions.Compiled);
        private static readonly Regex _hasSpecial = new (@"([!@#$%*()&\/=\?_\.,:;\-])", RegexOptions.Compiled);

        public int HasNumeric { get; }

        public int HasLetter { get; }

        public int HasCapital { get; }

        public int HasSpecial { get; }

        public int Total => HasNumeric + HasLetter + HasCapital + HasSpecial;

        private readonly string _password;

        public ComplexityScore(string password)
        {
            _password = password;

            HasNumeric = Score(password, _numeric);
            HasLetter = Score(password, _hasLetter);
            HasCapital = Score(password, _hasCapital);
            HasSpecial = Score(password, _hasSpecial);
        }

        public void ValidOrFail()
        {
            if (_password.Length < MinLength)
            {
                throw new InputValidationException(
                    $"Number of symbols in the new password must be not less than {MinLength} symbols");
            }

            if (EnoughComplexity())
            {
                return;
            }

            throw new InvalidPassError(this).Exception();
        }

        public bool EnoughComplexity()
        {
            return Total >= MinSuccess;
        }

        private static int Score(string pass, Regex regexToCheck)
        {
            return regexToCheck.IsMatch(pass) ? 1 : 0;
        }
    }
}