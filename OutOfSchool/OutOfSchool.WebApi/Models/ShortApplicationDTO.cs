using System;
using System.ComponentModel.DataAnnotations;

using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ShortApplicationDto
    {
        public Guid Id { get; set; }

        [Required]
        // TODO: remove range and use IsDefined instead
        [Range(1, 5, ErrorMessage = "Status should be from 1 to 5")]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [MaxLength(500)]
        public string RejectionMessage { get; set; }
    }
}
