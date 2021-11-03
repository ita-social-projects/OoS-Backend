using System;
using System.ComponentModel.DataAnnotations;

using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class DateTimeRange
    {
        public long Id { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public Guid WorkshopId { get; set; }

        [Required]
        public DaysBitMask Workdays { get; set; }
    }
}