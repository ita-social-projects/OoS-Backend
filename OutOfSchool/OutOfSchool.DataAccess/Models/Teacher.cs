﻿#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OutOfSchool.Services.Models
{
    public class Teacher
    {
        public long TeacherId { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^([^0-9]*)$", ErrorMessage = "First name cannot contains digits")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^([^0-9]*)$", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Middle name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^([^0-9]*)$", ErrorMessage = "Middle name cannot contains digits")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Birthday is required")]
        [DataType(DataType.Date)]
        public DateTime BirthDay { get; set; }
        
        [MaxLength(300)]
        public string? Description { get; set; }
        
        public string? Image { get; set; }
        
        public Workshop Workshop { get; set; }
    }
}