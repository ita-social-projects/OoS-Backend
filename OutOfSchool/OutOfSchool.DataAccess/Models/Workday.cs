using System;

namespace OutOfSchool.Services.Models
{
    public class Workday : IEquatable<Workday>
    {
        public long Id { get; set; }

        public long DateTimeRangeId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public bool Equals(Workday other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && DayOfWeek == other.DayOfWeek && DateTimeRangeId == other.DateTimeRangeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Workday) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, (int)DayOfWeek, DateTimeRangeId);
        }
    }
}