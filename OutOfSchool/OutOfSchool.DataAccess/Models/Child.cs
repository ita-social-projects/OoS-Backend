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
        [RegularExpression(@"[\u0400-\u04FF\-\']*$", ErrorMessage = "First name cannot contains digits")]
        public string FirstName { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Last name is required")]
        [RegularExpression(@"[\u0400-\u04FF\-\']*$", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Patronymic name is required")]
        [RegularExpression(@"[\u0400-\u04FF\-\']*$", ErrorMessage = "Patronymic cannot contains digits")]
        public string Patronymic { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public long AddressId { get; set; }

        public virtual Address Address { get; set; }

        public virtual Parent Parent { get; set; }

        public virtual SocialGroup SocialGroup { get; set; }

        public long ParentId { get; set; }

        public long SocialGroupId { get; set; }

        public virtual BirthCertificate BirthCertificate { get; set; }
    }
}