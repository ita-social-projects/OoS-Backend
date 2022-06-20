using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Util.JsonTools;

namespace OutOfSchool.WebApi.Models
{
    /// <inheritdoc/>>
    public class WorkshopFilter : OffsetFilter
    {
        public List<Guid> Ids { get; set; } = new List<Guid>();

        public string SearchText { get; set; } = string.Empty;

        public string OrderByField { get; set; } = OrderBy.Rating.ToString();

        [Range(0, 100, ErrorMessage = "Field value should be in a range from 0 to 100")]
        public int MinAge { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "Field value should be in a range from 0 to 100")]
        public int MaxAge { get; set; } = 100;

        public bool IsFree { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Field value should be in a range from 0 to 2 147 483 647")]
        public int MinPrice { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Field value should be in a range from 0 to 2 147 483 647")]
        public int MaxPrice { get; set; } = int.MaxValue;

        public List<long> DirectionIds { get; set; } = new List<long>();

        public string City { get; set; } = string.Empty;

        public bool WithDisabilityOptions { get; set; } = false;

        public List<DaysBitMask> Workdays { get; set; } = new List<DaysBitMask>();

        [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
        [JsonConverter(typeof(TimespanConverter))]
        public TimeSpan MinStartTime { get; set; } = new TimeSpan(0, 0, 0);

        [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
        [JsonConverter(typeof(TimespanConverter))]
        public TimeSpan MaxStartTime { get; set; } = new TimeSpan(23, 59, 59);

        public decimal Latitude { get; set; } = 0;

        public decimal Longitude { get; set; } = 0;

        public WorkshopStatus Status { get; set; } = 0;
    }
}
