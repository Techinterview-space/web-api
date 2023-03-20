using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Domain.Extensions;

public static class EnumHelper
{
    public static IEnumerable<T> Values<T>()
        where T : struct
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    public static TEnum ToEnum<TEnum>(this string value, TEnum defaultValue = default)
        where TEnum : struct
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        value = value.Trim();

        return Enum.TryParse<TEnum>(value, true, out TEnum result)
            ? result :
            defaultValue;
    }

    public static IReadOnlyCollection<T> Attributes<T>(this Enum enumVal)
        where T : Attribute
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString());
        return memInfo[0].GetCustomAttributes(typeof(T), false)
            .Select(x => (T)x)
            .ToArray();
    }

    /// <summary>
    /// Gets an attribute on an enum field value.
    /// </summary>
    /// <typeparam name="T">The type of the attribute you want to retrieve.</typeparam>
    /// <param name="enumVal">The enum value.</param>
    /// <returns>The attribute of type T that exists on the enum value.</returns>
    /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
    public static T AttributeOrNull<T>(this Enum enumVal)
        where T : Attribute
    {
        return enumVal.Attributes<T>().FirstOrDefault();
    }

    public static bool HasAttribute<T>(this Enum enumVal)
        where T : Attribute
    {
        return enumVal.Attributes<T>().Any();
    }

    public static string Description(this Enum genericEnum)
    {
        return genericEnum
            .AttributeOrNull<DescriptionAttribute>()?
            .Description ?? genericEnum.ToString();
    }
}