using System.Text.RegularExpressions;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class SectionDayOfWeek
    {
        public long SectionDayOfWeekId { get; set; }
        public Section Section { get; set; }
        public DaysOfWeek Day { get; set; }
    }
}