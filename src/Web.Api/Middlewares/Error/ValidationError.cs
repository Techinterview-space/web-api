using Domain.Validation;

namespace Web.Api.Middlewares.Error;

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