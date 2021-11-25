using System;
using Nest;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class DateTimeRangeES
    {
        public long Id { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        [Keyword]
        public Guid WorkshopId { get; set; }

        public string Workdays { get; set; }
    }
}