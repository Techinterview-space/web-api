using System;
using MG.Utils.Abstract;
using MG.Utils.Abstract.Dates;
using MG.Utils.Abstract.Dates.Interfaces;
using MG.Utils.Abstract.Exceptions;

namespace MG.Utils.Validation;

public static class EntityValidatorExtensions
{
    /// <summary>
    /// Asserts that a model entity is valid by it's annotation validation attributes.
    /// </summary>
    /// <typeparam name="T">Generic.</typeparam>
    /// <param name="entity">Entity instance.</param>
    /// <returns>The entity.</returns>
    /// <exception cref="Validation.Exception.EntityInvalidException">If the entity is not valid.</exception>
    public static T ThrowIfInvalid<T>(this T entity)
    {
        new EntityValidator<T>(entity).ThrowIfInvalid();
        return entity;
    }

    /// <summary>
    /// Throws if date range is out of allowed limits.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="instance">The instance to check.</param>
    /// <returns>The entity itself.</returns>
    /// <exception cref="InvalidDateRangeException">If dates are not valid.</exception>
    public static T ThrowIfDateRangeIsOutOfAllowedLimits<T>(this T instance)
        where T : IHasFromToDates
    {
        instance.ThrowIfNull(nameof(instance));

        if (instance.From.Earlier(TimeRange.Min) || instance.From.Later(TimeRange.Max))
        {
            throw new InvalidDateRangeException(
                $"{nameof(TimeRange.From)} is not within allowed range");
        }

        if (instance.To.Earlier(TimeRange.Min) || instance.To.Later(TimeRange.Max))
        {
            throw new InvalidDateRangeException(
                $"{nameof(TimeRange.To)} is not within allowed range");
        }

        return instance;
    }

    public static T ThrowIfDateRangeIsNotValid<T>(this T instance, bool toIsRequired)
        where T : IHasFromToDates
    {
        if (instance.From.Earlier(TimeRange.Min))
        {
            throw new InvalidDateRangeException(
                $"A From Date of the {typeof(T).Name} should not be earlier than MinValue");
        }

        if (toIsRequired && !instance.To.HasValue)
        {
            throw new ArgumentNullException(nameof(instance.To), $"A To Date of the {typeof(T).Name} should not be null");
        }

        if (instance.RangeReversed(toIsRequired))
        {
            throw new InvalidDateRangeException("To date cannot be greater than From date");
        }

        if (instance.To.HasValue && instance.To.Value.Later(TimeRange.Max))
        {
            throw new InvalidDateRangeException(
                $"A To Date of the {typeof(T).Name} should not be later than MaxValue");
        }

        return instance;
    }

    public static bool RangeReversed<T>(this T instance, bool toIsRequired)
        where T : IHasFromToDates
    {
        return (toIsRequired || instance.To.HasValue) && instance.From.Later(instance.ToOrFail());
    }
}