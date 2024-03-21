﻿using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthCommon.ViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression(
        Constants.PasswordRegexViewModel,
        ErrorMessage = Constants.PasswordValidationErrorMessage)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Password confirmation is required")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords doesn't match")]
    public string ConfirmNewPassword { get; set; }

    public string ReturnUrl { get; set; }
}