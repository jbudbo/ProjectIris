namespace System;

internal static class StringExtensions
{
    /// <summary>
    /// Returns a given string with the first character in Upper Case
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string RaiseFirstChar(this string source)
        => string.IsNullOrWhiteSpace(source) ? source : string.Concat(char.ToUpper(source[0]), source?[1..]);
}
