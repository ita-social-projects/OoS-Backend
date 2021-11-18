using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ShortUserDto
    {
        public string Id { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(
            @"([\d]{9})",
            ErrorMessage = "Phone number format is incorrect. Example: 501234567")]
        [DisplayFormat(DataFormatString = "{0:+380 XX-XXX-XX-XX}")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; }

        public string Role { get; set; }

        public bool IsRegistered { get; set; }
    }
}
