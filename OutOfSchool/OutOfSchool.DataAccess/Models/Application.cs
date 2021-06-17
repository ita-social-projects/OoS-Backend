using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Application
    {
        public long Id { get; set; }

        [Range(0, 2, ErrorMessage = "Status should be from 0 to 2")]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [Required]
        public DateTime CreationTime { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Workshop id should be grater than 0")]
        public long WorkshopId { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Child id should be grater than 0")]
        public long ChildId { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Parent id should be grater than 0")]
        public long ParentId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public virtual Child Child { get; set; }

        public virtual Parent Parent { get; set; }
    }
}
