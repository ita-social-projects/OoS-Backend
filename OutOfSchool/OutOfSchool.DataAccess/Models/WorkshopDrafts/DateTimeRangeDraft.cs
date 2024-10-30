using System;
using System.Collections.Generic;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.WorkshopDrafts;

public class DateTimeRangeDraft
{
    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public List<DaysBitMask> Workdays { get; set; }
}