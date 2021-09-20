using System;
using System.Collections.Generic;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Util.CustomComparers
{
    public class DateTimeRangeComparerWithoutFK : IEqualityComparer<DateTimeRange>
    {
        public bool Equals(DateTimeRange x, DateTimeRange y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.Id == y.Id && x.StartTime.Equals(y.StartTime) && x.EndTime.Equals(y.EndTime);
        }

        public int GetHashCode(DateTimeRange obj)
        {
            return HashCode.Combine(obj.Id, obj.StartTime, obj.EndTime);
        }
    }
}