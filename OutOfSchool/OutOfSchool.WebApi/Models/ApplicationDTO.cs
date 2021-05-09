using OutOfSchool.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class ApplicationDTO
    {
        public long Id { get; set; }

        public ApplicationStatus Status { get; set; }

        public long WorkshopId { get; set; }

        public long ChildId { get; set; }

        public string UserId { get; set; }
    }
}
