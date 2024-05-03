using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.BusinessLogic.Models;

public class BaseUserDto
{
    public string Id { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName]
    public string LastName { get; set; }

    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName]
    public string MiddleName { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName]
    public string FirstName { get; set; }
}
