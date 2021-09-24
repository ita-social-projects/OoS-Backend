using System;
using System.Collections.Generic;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class DateTimeRange
    {
        public long Id { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public long WorkshopId { get; set; }

        public List<DaysBitMask> Workdays { get; set; }
    }
}