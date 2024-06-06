using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Domain.Attributes;
using Domain.Entities.Enums;

namespace Domain.Extensions;

public static class EnumHelper
{
    public static IEnumerable<T> Values<T>()
        where T : struct
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    public static TEnum ToEnum<TEnum>(
        this string value,
        TEnum defaultValue = default)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        value = value.Trim();

        return Enum.TryParse(value, true, out TEnum result)
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

    public static GradeGroup? GetGroupName<TEnum>(this TEnum? enumValue)
        where TEnum : struct, Enum
    {
        if (enumValue == null)
        {
            return null;
        }

        var memberInfo = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault();
        var attribute = memberInfo?.GetCustomAttribute<GroupAttribute>();
        return attribute?.GroupName;
    }

    public static string ToCustomString(this GradeGroup enumValue)
    {
        return enumValue switch
        {
            GradeGroup.Trainee => "Стажеры",
            GradeGroup.Junior => "Джуны",
            GradeGroup.Middle => "Миддлы",
            GradeGroup.Senior => "Сеньоры",
            GradeGroup.Lead => "Лиды",
            _ => throw new ArgumentException($"No string mapping found for enum value {enumValue}")
        };
    }
}