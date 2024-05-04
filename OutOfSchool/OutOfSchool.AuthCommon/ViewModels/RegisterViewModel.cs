using System.ComponentModel.DataAnnotations;
using OutOfSchool.AuthCommon.Validators;

namespace OutOfSchool.AuthCommon.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "FirstName is required")]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.NameErrorMessage)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.NameErrorMessage)]
    public string LastName { get; set; }

    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.NameErrorMessage)]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [CustomPasswordValidation]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Password confirmation is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords doesn't match")]
    public string ConfirmPassword { get; set; }

    [DataType(DataType.EmailAddress)]
    [MaxLength(256)]
    [Required(ErrorMessage = "Email is required")]
    [RegularExpression(
        Constants.EmailRegexViewModel,
        ErrorMessage = "Email is not valid")]
    public string Email { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset CreatingTime { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset? LastLogin { get; set; }

    public string ReturnUrl { get; set; }

    public string? Role { get; set; }

    public bool ProviderRegistration { get; set; }
}