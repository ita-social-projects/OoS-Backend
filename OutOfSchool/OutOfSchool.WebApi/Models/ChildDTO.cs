using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ChildDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\W\-\']*", ErrorMessage = "First name cannot contains digits")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\W\-\']*", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Patronymic is required")]
        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\W\-\']*", ErrorMessage = "Patronymic cannot contains digits")]
        public string Patronymic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birthday is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = default;

        public Gender Gender { get; set; } = default;

        public long ParentId { get; set; } = default;

        public long SocialGroupId { get; set; } = default;

        public AddressDto Address { get; set; }

        public BirthCertificateDTO BirthCertificate { get; set; }
    }
}