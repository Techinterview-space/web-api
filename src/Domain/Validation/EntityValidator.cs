using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using EntityInvalidException = Domain.Validation.Exceptions.EntityInvalidException;

namespace Domain.Validation;

public class EntityValidator<T>
{
    private readonly T _entity;

    private readonly ICollection<string> _errors;

    private bool? _valid;

    public EntityValidator(T entity)
    {
        entity.ThrowIfNull(nameof(entity));
        _entity = entity;
        _errors = new List<string>();
    }

    public bool Valid()
    {
        _valid ??= ValidInternal();

        return _valid.Value;
    }

    /// <summary>
    /// Asserts that a model entity is valid by it's annotation validation attributes.
    /// </summary>
    /// <exception cref="EntityInvalidException">If the entity is not valid.</exception>
    public void ThrowIfInvalid()
    {
        if (!Valid())
        {
            throw EntityInvalidException.FromInstance<T>(_errors);
        }
    }

    private bool ValidInternal()
    {
        // Copied from https://stackoverflow.com/a/2193988
        var type = typeof(T);
        var meta = type.GetCustomAttributes(false).OfType<MetadataTypeAttribute>().FirstOrDefault();
        if (meta != null)
        {
            type = meta.MetadataClassType;
        }

        var propertyInfo = type.GetProperties();
        foreach (var info in propertyInfo)
        {
            var attributes = info.GetCustomAttributes(false).OfType<ValidationAttribute>();
            foreach (ValidationAttribute attribute in attributes)
            {
                PropertyInfo objPropInfo = _entity.GetType().GetProperty(info.Name)
                                           ?? throw new InvalidOperationException($"No property info '{info.Name}'");

                if (attribute.GetValidationResult(objPropInfo.GetValue(_entity, null), new ValidationContext(_entity)) != ValidationResult.Success)
                {
                    _errors.Add($"{info.Name} is invalid by attribute {attribute.GetType().Name}");
                }
            }
        }

        return !_errors.Any();
    }
}