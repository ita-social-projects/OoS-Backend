using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthCommon.ViewModels;

public class ChangeEmailViewModel
{
    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    public string CurrentEmail { get; set; }

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }

    public string ReturnUrl { get; set; }

    public string Submit { get; set; }
}