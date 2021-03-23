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
        public string FirstName { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Patronymic name is required")]
        public string Patronymic { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public virtual Parent Parent { get; set; }

        public virtual SocialGroup SocialGroup { get; set; }

        public long ParentId { get; set; }

        public long SocialGroupId { get; set; }
    }
}