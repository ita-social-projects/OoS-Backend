using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Util;

public static class DaysBitMaskHelper
{
    /// <summary>
    /// Maps <see cref="IEnumerable{DaysBitMask}"/> of <see cref="DaysBitMask"/> into single enum using bitwise OR.
    /// </summary>
    /// <param name="daysList">Source.</param>
    /// <returns>Merged enum.</returns>
    public static DaysBitMask ToDaysBitMask(this IEnumerable<DaysBitMask> daysList)
    {
        return daysList.Aggregate((prev, next) => prev | next);
    }

    /// <summary>
    /// Splits <see cref="DaysBitMask"/> into <see cref="IEnumerable{DaysBitMask}"/>.
    /// </summary>
    /// <param name="daysBitMask">Source.</param>
    /// <returns>Splitted enumerable.</returns>
    public static IEnumerable<DaysBitMask> ToDaysBitMaskEnumerable(this DaysBitMask daysBitMask)
    {
        return Enum.GetValues(typeof(DaysBitMask))
            .Cast<DaysBitMask>().ToList()
            .Where(amenity => amenity != 0 && daysBitMask.HasFlag(amenity));
    }
}