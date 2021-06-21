using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ShortApplicationDTO
    {
        public long Id { get; set; }

        [Required]
        [Range(0, 2, ErrorMessage = "Status should be from 0 to 2")]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    }
}
