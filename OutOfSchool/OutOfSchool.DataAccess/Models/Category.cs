﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class Category
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(60)]
        [MinLength(1)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public virtual List<Subcategory> Subcategories { get; set; }
    }
}