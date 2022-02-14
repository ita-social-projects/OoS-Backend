using System;
using System.Collections.Generic;
using System.Linq;
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
            return x.Id == y.Id && x.StartTime.Equals(y.StartTime) && x.EndTime.Equals(y.EndTime) && x.Workdays == y.Workdays;
        }

        public int GetHashCode(DateTimeRange obj)
        {
            var hash = default(HashCode);
            hash.Add(obj.Id);
            hash.Add(obj.StartTime);
            hash.Add(obj.EndTime);
            hash.Add(obj.Workdays);
            return hash.ToHashCode();
        }
    }
}