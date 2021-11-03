using System;

using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Application
    {
        public Guid Id { get; set; }

        public ApplicationStatus Status { get; set; }

        public DateTimeOffset CreationTime { get; set; }

        public Guid WorkshopId { get; set; }

        public Guid ChildId { get; set; }

        public Guid ParentId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public virtual Child Child { get; set; }

        public virtual Parent Parent { get; set; }
    }
}
