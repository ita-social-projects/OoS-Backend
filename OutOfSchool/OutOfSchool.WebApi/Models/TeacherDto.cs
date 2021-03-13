using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class TeacherDTO
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

        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;
    }
}