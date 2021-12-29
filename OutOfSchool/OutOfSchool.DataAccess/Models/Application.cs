using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Application
    {
        public Guid Id { get; set; }

        public ApplicationStatus Status { get; set; }

        [MaxLength(500)]
        public string RejectionMessage { get; set; }

        public DateTimeOffset CreationTime { get; set; }

        public DateTimeOffset? ApprovedTime { get; set; }

        public Guid WorkshopId { get; set; }

        public Guid ChildId { get; set; }

        public Guid ParentId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public virtual Child Child { get; set; }

        public virtual Parent Parent { get; set; }
    }
}
