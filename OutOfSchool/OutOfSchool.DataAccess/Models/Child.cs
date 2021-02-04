using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Child
    {

        public long Id { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Middle name is required")]
        public string MiddleName { get; set; }

        public DateTime DateOfBirth { get; set; } = default;

        public Gender Gender { get; set; } = default;

        public virtual Parent Parent { get; set; }

        public virtual SocialGroup SocialGroup { get; set; }

    }
}
