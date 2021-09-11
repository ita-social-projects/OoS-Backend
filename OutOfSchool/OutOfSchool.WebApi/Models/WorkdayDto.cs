using System;

namespace OutOfSchool.WebApi.Models
{
    public class WorkdayDto
    {
        public long Id { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
    }
}