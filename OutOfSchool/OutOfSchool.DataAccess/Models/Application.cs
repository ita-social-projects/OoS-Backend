using OutOfSchool.Services.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Models
{
    public class Application
    {
        public long Id { get; set; }

        public ApplicationStatus Status { get; set; }

        public long WorkshopId { get; set; }

        public long ChildId { get; set; }

        public string UserId { get; set; }
        
        public virtual Workshop Workshop { get; set; }

        public virtual Child Child { get; set; }

        public virtual User User { get; set; }
    }
}
