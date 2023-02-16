using System;

namespace MG.Utils.Exceptions
{
    public class InputValidationException : InvalidOperationException
    {
        public InputValidationException(string message)
            : base(message)
        {
        }

        public InputValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}