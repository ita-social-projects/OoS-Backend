using System;
using System.Collections.Generic;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Util.CustomComparers
{
    public class WorkdayComparerWithoutFK : IEqualityComparer<Workday>
    {
        public bool Equals(Workday x, Workday y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.Id == y.Id && x.DayOfWeek == y.DayOfWeek;
        }

        public int GetHashCode(Workday obj)
        {
            return HashCode.Combine(obj.Id, (int)obj.DayOfWeek);
        }
    }
}