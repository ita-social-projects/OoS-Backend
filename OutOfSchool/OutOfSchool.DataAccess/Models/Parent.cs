﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class Parent
    {
        public Parent()
        {
            Children = new List<Child>();
        }

        public long Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Middle name is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "Middle name cannot contains digits")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; } = string.Empty;

        public virtual IReadOnlyCollection<Child> Children { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual User User { get; set; }
    }
}