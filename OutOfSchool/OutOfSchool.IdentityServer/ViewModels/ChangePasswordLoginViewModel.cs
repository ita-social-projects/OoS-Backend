using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.IdentityServer.ViewModels
{
    public class ChangePasswordLoginViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [RegularExpression(
            Constants.PasswordRegexViewModel,
            ErrorMessage = "Password must contain at least one capital, number and symbol(@$!%*?&).")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords doesn't match")]
        public string ConfirmNewPassword { get; set; }

        public string ReturnUrl { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CurrentPassword.Equals(NewPassword))
            {
                yield return new ValidationResult("Current password cannot be equal to new password");
            }
        }
    }
}