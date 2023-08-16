namespace OutOfSchool.WebApi.Extensions;

public static class DoubleExtension
{
    public static string ToStringWithDotSeparator(this double value) =>
        value.ToString().Replace(",", ".");
}
