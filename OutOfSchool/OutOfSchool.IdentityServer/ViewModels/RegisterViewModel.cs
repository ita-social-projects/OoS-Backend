using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.IdentityServer.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
            ErrorMessage = "Password must contain at least one capital, number and symbol(@$!%*?&).")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords doesn't match")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(
            @"([0-9]{2})([-]?)([0-9]{3})([-]?)([0-9]{2})([-]?)([0-9]{2})",
            ErrorMessage = "Phone number format is incorrect. Example: +380XX-XXX-XX-XX")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset CreatingTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset? LastLogin { get; set; }

        public string ReturnUrl { get; set; }

        public string Role { get; set; }

        public bool ProviderRegistration { get; set; }
    }
}