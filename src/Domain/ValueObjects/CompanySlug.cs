using System;

namespace Domain.ValueObjects;

public record CompanySlug
{
    private readonly string _name;

    public CompanySlug(
        string name)
    {
        name = new KebabCaseSlug(name).ToString();
        var guidPart = Guid.NewGuid().ToString("N")[..8];

        _name = $"{name}-{guidPart}";
    }

    public override string ToString()
    {
        return _name;
    }
}