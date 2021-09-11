using System;
using System.Collections.Generic;

namespace OutOfSchool.Services.Models
{
    public class DateTimeRange : IEquatable<DateTimeRange>
    {
        public long Id { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public long WorkshopId { get; set; }

        public virtual List<Workday> Workdays { get; set; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DateTimeRange) obj);
        }
        
        public bool Equals(DateTimeRange other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime)
                   && WorkshopId == other.WorkshopId
                   ; // TODO - move to 2  hash sets
        }
        //
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, StartTime, EndTime, WorkshopId);
        }
    }
}