using System;
using System.Collections.Generic;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.WorkshopDrafts;

/// <summary>
///     Will be stored as nested objects in the JSON format of the workshop draft.
///     This entity is specific to the draft and can be hard-deleted if the draft is removed.
/// </summary>
public class DateTimeRangeDraft
{
    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public List<DaysBitMask> Workdays { get; set; }
}