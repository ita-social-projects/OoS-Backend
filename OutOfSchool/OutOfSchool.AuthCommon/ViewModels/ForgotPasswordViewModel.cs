﻿using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthCommon.ViewModels;

public class ForgotPasswordViewModel
{
    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }

    public string ReturnUrl { get; set; }
}