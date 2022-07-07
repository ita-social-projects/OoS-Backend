using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.IdentityServer.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "FirstName is required")]
    [MaxLength(30)]
    [RegularExpression(
        Constants.NameRegexViewModel,
        ErrorMessage = Constants.NameErrorMessage)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    [MaxLength(30)]
    [RegularExpression(
        Constants.NameRegexViewModel,
        ErrorMessage = Constants.NameErrorMessage)]
    public string LastName { get; set; }

    [MaxLength(30)]
    [RegularExpression(
        Constants.NameRegexViewModel,
        ErrorMessage = Constants.NameErrorMessage)]
    public string MiddleName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression(
        Constants.PasswordRegexViewModel,
        ErrorMessage = "Password must contain at least one capital, number and symbol(@$!%*?&).")]
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

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(
        Constants.PhoneNumberRegexViewModel,
        ErrorMessage = Constants.PhoneErrorMessage)]
    public string PhoneNumber { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset CreatingTime { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset? LastLogin { get; set; }

    public string ReturnUrl { get; set; }

    public string Role { get; set; }

    public bool ProviderRegistration { get; set; }
}