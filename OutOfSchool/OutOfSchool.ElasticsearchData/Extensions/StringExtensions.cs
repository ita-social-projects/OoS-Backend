using OutOfSchool.Common.Extensions;

namespace OutOfSchool.ElasticsearchData.Extensions;

public static class StringExtensions
{
    public static string FirstCharToLowerCase(this string str)
    {
        if (!str.IsNullOrEmpty() && char.IsUpper(str[0]))
        {
            return str.Length == 1
                ? char.ToLower(str[0]).ToString()
                : char.ToLower(str[0]) + str[1..];
        }

        return str;
    }
}