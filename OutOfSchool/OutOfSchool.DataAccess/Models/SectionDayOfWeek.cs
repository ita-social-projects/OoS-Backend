using System;
using System.Text.RegularExpressions;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class SectionDayOfWeek
    {
        public long Id { get; set; }
        public Workshop Workshop { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
    }
}