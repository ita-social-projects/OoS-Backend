using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models;

public class BaseUserDto
{
    public string Id { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(
        Constants.PhoneNumberRegexModel,
        ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    public string LastName { get; set; }

    public string MiddleName { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    public string FirstName { get; set; }
}
