﻿using System;
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
        [Range(1, long.MaxValue, ErrorMessage = "Workshop id should be grater than 0")]
        public long WorkshopId { get; set; }

        [Required]
        public Guid ChildId { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Parent id should be grater than 0")]
        public long ParentId { get; set; }

        public WorkshopDTO Workshop { get; set; }

        public ChildDto Child { get; set; }

        public ParentDTO Parent { get; set; }
    }
}
