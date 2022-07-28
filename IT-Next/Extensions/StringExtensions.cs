using System.Text.RegularExpressions;

namespace IT_Next.Extensions;

public static class StringExtensions
{
    public static string TrimExtraSpaces(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var newValue = value.Trim();
        newValue = Regex.Replace(newValue, @"\s+", " ");

        return newValue;
    }
}
