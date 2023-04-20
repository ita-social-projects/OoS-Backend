using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models;

public class ParentDtoWithContactInfo : ParentDTO
{
    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PhoneNumber { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string FirstName { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender Gender { get; set; }
}