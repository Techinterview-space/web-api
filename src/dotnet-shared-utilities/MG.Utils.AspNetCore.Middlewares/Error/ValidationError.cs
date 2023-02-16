using MG.Utils.Abstract;

namespace MG.Utils.AspNetCore.Middlewares.Error
{
    public class ValidationError
    {
        public ValidationError(string name, string description)
        {
            Name = name.ThrowIfNullOrEmpty(nameof(name));
            Description = description.ThrowIfNullOrEmpty(nameof(description));
        }

        public string Name { get; }

        public string Description { get; }
    }
}