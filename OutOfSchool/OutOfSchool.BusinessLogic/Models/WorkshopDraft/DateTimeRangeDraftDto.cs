using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

public class DateTimeRangeDraftDto
{
    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [Required]
    public HashSet<DaysBitMask> Workdays { get; set; }
}
