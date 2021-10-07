using System;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class DateTimeRangeES
    {
        public long Id { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public long WorkshopId { get; set; }

        public string Workdays { get; set; }
    }
}