using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ChildDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "First name cannot contains digits")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "Middle name cannot contains digits")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = default;

        [Required(ErrorMessage = "Gender is required")]
        public Gender Gender { get; set; } = default;

        [Required(ErrorMessage = "Parent Id is required")]
        public long ParentId { get; set; } = default;

        public long? SocialGroupId { get; set; } = default;

        public BirthCertificateDto BirthCertificate { get; set; }
    }
}