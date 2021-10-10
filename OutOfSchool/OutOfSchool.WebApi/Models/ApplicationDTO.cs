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

        public DateTimeOffset CreationTime { get; set; }

        [Required]
        public Guid WorkshopId { get; set; }

        [Required]
        public Guid ChildId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Parent id should be grater than 0")]
        public Guid ParentId { get; set; }

        public WorkshopDTO Workshop { get; set; }

        public ChildDto Child { get; set; }

        public ParentDTO Parent { get; set; }
    }
}
