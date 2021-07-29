using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ShortApplicationDto
    {
        public long Id { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Status should be from 1 to 5")]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    }
}
