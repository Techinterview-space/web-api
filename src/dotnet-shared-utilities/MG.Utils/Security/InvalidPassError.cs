using System;
using System.Collections.Generic;
using MG.Utils.Exceptions;

namespace MG.Utils.Security
{
    internal class InvalidPassError
    {
        private readonly ComplexityScore _score;

        public InvalidPassError(ComplexityScore score)
        {
            _score = score;
        }

        public Exception Exception()
        {
            if (_score.EnoughComplexity())
            {
                throw new InvalidOperationException("The pass has enough complexity");
            }

            var errors = new List<string>();
            if (_score.HasCapital == 0)
            {
                errors.Add(
                    errors.Count > 0
                        ? " or uppercase letters"
                        : " uppercase letters");
            }

            if (_score.HasNumeric == 0)
            {
                errors.Add(
                    errors.Count > 0
                        ? " or numbers"
                        : " numbers");
            }

            if (_score.HasSpecial == 0)
            {
                errors.Add(
                    errors.Count > 0
                        ? " or special symbols"
                        : " special symbols");
            }

            if (_score.HasLetter == 0)
            {
                errors.Add(
                    errors.Count > 0
                        ? " or lowercase letters"
                        : " lowercase letters");
            }

            string message = "The password must contain";
            errors.ForEach(x => message += x);

            return new InputValidationException(message);
        }
    }
}