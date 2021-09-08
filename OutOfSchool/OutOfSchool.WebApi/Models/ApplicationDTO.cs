using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ApplicationDto
    {
        public long Id { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public DateTimeOffset CreationTime { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Workshop id should be grater than 0")]
        public long WorkshopId { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Child id should be grater than 0")]
        public long ChildId { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Parent id should be grater than 0")]
        public long ParentId { get; set; }

        public WorkshopDTO Workshop { get; set; }

        public ChildDto Child { get; set; }

        public ParentDTO Parent { get; set; }
    }
}
