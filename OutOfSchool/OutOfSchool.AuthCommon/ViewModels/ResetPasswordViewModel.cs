﻿using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthCommon.ViewModels;

public class ResetPasswordViewModel
{
    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }

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

    [Required]
    public string Token { get; set; }
}