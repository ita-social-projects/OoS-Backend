using System;

namespace OutOfSchool.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime NextDayStart(this DateTime value) =>
        value.Date == DateTime.MaxValue.Date ? DateTime.MaxValue : value.Date.AddDays(1);
}
