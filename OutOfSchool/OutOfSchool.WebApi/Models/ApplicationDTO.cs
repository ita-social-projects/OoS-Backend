﻿using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ApplicationDto
    {
        public long Id { get; set; }

        [Range(1, 5, ErrorMessage = "Status should be from 1 to 5")]
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
