using System;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ApplicationDto
    {
        public Guid Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [MaxLength(500)]
        public string RejectionMessage { get; set; }

        public DateTimeOffset CreationTime { get; set; }

        public DateTimeOffset? ApprovedTime { get; set; }

        [Required]
        public Guid WorkshopId { get; set; }

        [Required]
        public Guid ChildId { get; set; }

        public Guid ParentId { get; set; }

        public WorkshopCard Workshop { get; set; }

        public ChildDto Child { get; set; }

        public ParentDTO Parent { get; set; }
    }
}
