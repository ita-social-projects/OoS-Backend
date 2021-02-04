using System.Text.RegularExpressions;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class GroupDayOfWeek
    {
        public long GroupDayOfWeekId { get; set; }
        public Group Group { get; set; }
        public DaysOfWeek Day { get; set; }
    }
}