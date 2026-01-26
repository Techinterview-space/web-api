using System;

namespace Domain.Validation.Exceptions
{
    public class InvalidDateRangeException : InvalidOperationException
    {
        public InvalidDateRangeException(string message)
            : base(message)
        {
        }

        public InvalidDateRangeException()
            : this("To date is null")
        {
        }

        public static InvalidDateRangeException CreateFromEntity<T>(long id)
        {
            return new ($"{typeof(T).Name} #Id:{id} has no To date");
        }
    }
}