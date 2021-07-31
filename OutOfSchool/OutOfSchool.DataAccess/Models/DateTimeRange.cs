using System;
using System.Collections.Generic;

namespace OutOfSchool.Services.Models
{
    public class DateTimeRange
    {
        public long Id { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public virtual List<Workday> Workdays { get; set; }
    }
}