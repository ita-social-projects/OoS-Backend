using System;

namespace OutOfSchool.Services.Models
{
    public class SectionDayOfWeek : IKeyedEntity<long>
    {
        public long Id { get; set; }

        public Workshop Workshop { get; set; }

        public DayOfWeek DayOfWeek { get; set; } = default;
    }
}