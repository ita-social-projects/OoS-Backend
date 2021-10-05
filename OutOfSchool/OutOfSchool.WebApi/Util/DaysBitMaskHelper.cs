using System;
using System.Collections.Generic;
using System.Linq;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Util
{
    public static class DaysBitMaskHelper
    {
        public static DaysBitMask ToDaysBitMask(this IEnumerable<DaysBitMask> daysList)
        {
            return daysList.Aggregate((prev, next) => prev | next);
        }

        public static List<DaysBitMask> ToDaysBitMaskList(this DaysBitMask daysBitMask)
        {
            return Enum.GetValues(typeof(DaysBitMask))
                .Cast<DaysBitMask>().ToList()
                .Where(amenity => amenity != 0 && daysBitMask.HasFlag(amenity)).ToList();
        }
    }
}