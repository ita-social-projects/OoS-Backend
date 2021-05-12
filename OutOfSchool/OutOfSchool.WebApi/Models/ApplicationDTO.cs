using OutOfSchool.Services.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class ApplicationDTO
    {
        public long Id { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [Range(1, long.MaxValue, ErrorMessage = "Workshop id should be grater than 0")]
        public long WorkshopId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Child id should be grater than 0")]
        public long ChildId { get; set; }

        public string UserId { get; set; }
    }
}
