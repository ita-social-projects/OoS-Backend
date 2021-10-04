using System;
using System.Collections.Generic;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class DateTimeRangeDto
    {
        public long Id { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public List<DaysBitMask> Workdays { get; set; }
    }
}