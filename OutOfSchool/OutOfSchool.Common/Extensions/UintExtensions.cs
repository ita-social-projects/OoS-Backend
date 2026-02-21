namespace OutOfSchool.Common.Extensions;

public static class UintExtensions
{
    public static uint GetMaxValueIfNullOrZero(this uint? value)
        => value is 0 or null ? uint.MaxValue : (uint)value;
}
