using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

public class DateTimeRangeDraftDto
{
    [JsonConverter(typeof(TimespanConverter))]
    public TimeSpan StartTime { get; set; }

    [JsonConverter(typeof(TimespanConverter))]
    public TimeSpan EndTime { get; set; }

    [Required]
    public List<DaysBitMask> Workdays { get; set; }
}
