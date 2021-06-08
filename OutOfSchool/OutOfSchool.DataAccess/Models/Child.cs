using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Child
    {
        public long Id { get; set; }

        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "First name cannot contains digits and special symbols")]
        public string FirstName { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "Last name cannot contains digits and special symbols")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "Middle name cannot contains digits and special symbols")]
        public string MiddleName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public virtual Parent Parent { get; set; }

        public virtual SocialGroup SocialGroup { get; set; }

        public long ParentId { get; set; }

        public long? SocialGroupId { get; set; }

        public virtual BirthCertificate BirthCertificate { get; set; }
    }
}