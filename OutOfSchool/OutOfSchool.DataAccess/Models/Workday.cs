using System;

namespace OutOfSchool.Services.Models
{
    public class Workday
    {
        public long Id { get; set; }

        public long DateTimeRangeId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
    }
}