using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Application
    {
        public long Id { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [Required]
        public DateTime CreationTime { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Workshop id should be grater than 0")]
        public long WorkshopId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Child id should be grater than 0")]
        public long ChildId { get; set; }

        public string UserId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public virtual Child Child { get; set; }

        public virtual User User { get; set; }
    }
}
