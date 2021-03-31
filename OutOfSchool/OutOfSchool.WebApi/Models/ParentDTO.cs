using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ParentDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(40)]
        [RegularExpression(@"[\u0400-\u04FF\-\']*$", ErrorMessage = "First name cannot contains digits")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Middle name is required")]
        [MaxLength(40)]
        [RegularExpression(@"[\u0400-\u04FF\-\']*$", ErrorMessage = "Middle name cannot contains digits")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(40)]
        [RegularExpression(@"[\u0400-\u04FF\-\']*$", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}
