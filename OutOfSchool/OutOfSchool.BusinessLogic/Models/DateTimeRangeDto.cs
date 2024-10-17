using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OutOfSchool.Services.Enums;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models;

public class DateTimeRangeDto
{
    public long Id { get; set; }

    [JsonConverter(typeof(TimespanConverter))]
    public TimeSpan StartTime { get; set; }

    [JsonConverter(typeof(TimespanConverter))]
    public TimeSpan EndTime { get; set; }

    [Required]
    public List<DaysBitMask> Workdays { get; set; }
}