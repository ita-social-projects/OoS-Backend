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
        [MaxLength(30)]
        [RegularExpression(@"^([^0-9]*)$", ErrorMessage = "First name cannot contains digits")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^([^0-9]*)$", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Patronymic name is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        [RegularExpression(@"^([^0-9]*)$", ErrorMessage = "Patronymic name cannot contains digits")]
        public string Patronymic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birthday is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = default;

        [Required(ErrorMessage = "Gender is required")] 
        public Gender Gender { get; set; }
        
        public long ParentId { get; set; } = default;

        public long SocialGroupId { get; set; } = default;
    }
}
