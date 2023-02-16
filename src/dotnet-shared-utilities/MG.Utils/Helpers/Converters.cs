using System;
using System.Collections.Generic;
using MG.Utils.Abstract;

namespace MG.Utils.Helpers
{
    public static class Converters
    {
        public static string AsJson<T>(this T instance)
        {
            return System.Text.Json.JsonSerializer.Serialize(instance);
        }

        public static T DeserializeAs<T>(this string @string)
        {
            @string.ThrowIfNull(nameof(@string));

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(@string);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Could not deserialize a string as {typeof(T).Name} type", ex);
            }
        }

        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> source)
        {
            source.ThrowIfNull(nameof(source));

            if (source is LinkedList<T> linked)
            {
                return linked;
            }

            return new LinkedList<T>(source);
        }
    }
}