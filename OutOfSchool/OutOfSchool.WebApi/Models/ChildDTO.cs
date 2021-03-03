using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChildDTO
    {
        public long Id { get; set; }
        
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
        public DateTime DateOfBirth { get; set; }
        
        public long ParentId { get; set; }
        
        public long SocialGroupId { get; set; }
    }
}
