using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthorizationServer.ViewModels;

public class AuthorizeViewModel
{
    [Display(Name = "Application")]
    public string ApplicationName { get; set; }
}