using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Util.JsonTools;

namespace OutOfSchool.WebApi.Models;

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