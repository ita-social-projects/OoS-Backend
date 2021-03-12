using OutOfSchool.Services.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OutOfSchool.Services.Models
{
    public class Child
    {
        public long Id { get; set; }
        
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;
        
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;
        
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Middle name is required")]
        public string MiddleName { get; set; } = string.Empty;
        
        public DateTime DateOfBirth { get; set; }
        
        public Gender Gender { get; set; }
        
        public virtual Parent Parent { get; set; }
        
        public virtual SocialGroup SocialGroup { get; set; }
    }
}
