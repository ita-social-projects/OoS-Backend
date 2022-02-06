﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class DirectionDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(100)]
        [MinLength(1)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}