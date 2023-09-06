using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace OutOfSchool.AuthCommon.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }

    public string ReturnUrl { get; set; }

    public IEnumerable<AuthenticationScheme> ExternalProviders { get; set; } = Array.Empty<AuthenticationScheme>();
}