#nullable enable
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.ModelsDto
{
    public class TeacherDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Middle name is required")]
        [DataType(DataType.Text)]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Short description is required")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public string? Image { get; set; }
    }
}