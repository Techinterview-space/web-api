namespace MG.Utils.Helpers
{
    /// <summary>
    /// copied from https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format.
    /// </summary>
    public static class StringHelpers
    {
        public static bool IsNullOrEmpty(this string @string)
        {
            return string.IsNullOrEmpty(@string);
        }
    }
}